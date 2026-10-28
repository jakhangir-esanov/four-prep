namespace FourPrep.Modules.Catalogue.Application.Imports.ImportRequirements;

using FluentValidation;
using FourPrep.Common.Domain;
using FourPrep.Common.Domain.Enums;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Catalogue.Domain.Programs;
using FourPrep.Modules.Catalogue.Domain.Universities;
using FourPrep.Modules.Catalogue.Application.Abstractions.Csv;
using FourPrep.Modules.Catalogue.Application.Abstractions.Data;
using FourPrep.Modules.Catalogue.Application.Abstractions.Authentication;
using FourPrep.Modules.Catalogue.Application.Programs.ReplaceProgramRequirements;

public sealed record ImportRequirementsCommand(Stream File) : ICommand<ImportResponse>;

internal sealed class ImportRequirementsCommandHandler(
    ICsvReader csvReader,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IUniversityRepository universityRepository,
    IProgramRepository programRepository,
    IValidator<ReplaceProgramRequirementsCommand> validator,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<ImportRequirementsCommand, ImportResponse>
{
    private static readonly ImportLayout Layout = new(
        [
            ImportColumns.UniversitySlug, ImportColumns.ProgramName, ImportColumns.IntakeTerm,
            ImportColumns.IntakeYear, ImportColumns.SourceUrl, ImportColumns.CheckedOn
        ],
        [
            ImportColumns.SatPolicy, ImportColumns.SatMidRangeLow, ImportColumns.SatMidRangeHigh,
            ImportColumns.MinimumGpa, ImportColumns.GpaScale, ImportColumns.Documents,
            ImportColumns.EssaysCount, ImportColumns.InterviewRequired
        ],
        AcceptsExamMinimums: true);

    public async Task<Result<ImportResponse>> Handle(ImportRequirementsCommand request, CancellationToken cancellationToken)
    {
        var table = ImportFile.Read(csvReader, request.File, Layout);
        if (table.IsFailure)
        {
            return Result.Failure<ImportResponse>(table.Error);
        }

        var problems = new ImportProblems();
        var groups = ImportLines.Group(ImportLines.ForPrograms(table.Data, problems, ToInput));

        foreach (var group in groups)
        {
            var replacement = new ReplaceProgramRequirementsCommand(Guid.Empty, group.Inputs);
            problems.AddFailures(validator.Validate(replacement), group.RowNumbers, ImportColumns.IntakeTerm);
        }

        var programs = await new ImportTargets(universityRepository, programRepository)
            .FindProgramsAsync(groups, problems, cancellationToken);

        if (problems.Any)
        {
            return Result.Failure<ImportResponse>(problems.ToError());
        }

        foreach (var (group, program) in programs)
        {
            program.ReplaceRequirements(
                group.Inputs.Select(requirement => requirement.ToRequirement()),
                currentUser.UserId,
                dateTimeProvider.UtcNow);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ImportResponse(table.Data.Rows.Count, 0, programs.Count);
    }

    private static AdmissionRequirementInput ToInput(ImportRow row) =>
        new(
            row.Choice<IntakeTerm>(ImportColumns.IntakeTerm) ?? default,
            row.Int(ImportColumns.IntakeYear) ?? default,
            row.ExamMinimums(),
            row.Choice<SatPolicy>(ImportColumns.SatPolicy),
            row.Int(ImportColumns.SatMidRangeLow),
            row.Int(ImportColumns.SatMidRangeHigh),
            row.Decimal(ImportColumns.MinimumGpa),
            row.Decimal(ImportColumns.GpaScale),
            row.List(ImportColumns.Documents),
            row.Int(ImportColumns.EssaysCount),
            row.Bool(ImportColumns.InterviewRequired),
            row.Text(ImportColumns.SourceUrl),
            row.Date(ImportColumns.CheckedOn) ?? default);
}
