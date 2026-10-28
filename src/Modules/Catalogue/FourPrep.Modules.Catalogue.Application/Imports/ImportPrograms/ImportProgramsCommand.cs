namespace FourPrep.Modules.Catalogue.Application.Imports.ImportPrograms;

using FluentValidation;
using FourPrep.Common.Domain;
using FourPrep.Common.Domain.Enums;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Catalogue.Domain.Programs;
using FourPrep.Modules.Catalogue.Domain.Universities;
using FourPrep.Modules.Catalogue.Application.Programs;
using FourPrep.Modules.Catalogue.Application.Abstractions.Csv;
using FourPrep.Modules.Catalogue.Application.Abstractions.Data;
using FourPrep.Modules.Catalogue.Application.Abstractions.Authentication;

public sealed record ImportProgramsCommand(Stream File) : ICommand<ImportResponse>;

internal sealed class ImportProgramsCommandHandler(
    ICsvReader csvReader,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IUniversityRepository universityRepository,
    IProgramRepository programRepository,
    IValidator<ProgramDetails> validator,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<ImportProgramsCommand, ImportResponse>
{
    private static readonly ImportLayout Layout = new(
        [
            ImportColumns.UniversitySlug, ImportColumns.Name, ImportColumns.DegreeLevel, ImportColumns.Field,
            ImportColumns.Language, ImportColumns.SourceUrl, ImportColumns.CheckedOn
        ],
        [ImportColumns.DurationMonths]);

    public async Task<Result<ImportResponse>> Handle(ImportProgramsCommand request, CancellationToken cancellationToken)
    {
        var table = ImportFile.Read(csvReader, request.File, Layout);
        if (table.IsFailure)
        {
            return Result.Failure<ImportResponse>(table.Error);
        }

        var problems = new ImportProblems();
        var lines = ImportLines.ForUniversities(table.Data, problems, ToDetails);

        foreach (var line in lines)
        {
            problems.AddFailures(validator.Validate(line.Input), line.RowNumber);
        }

        problems.AddDuplicates(
            lines.Select(line => (line.RowNumber, (line.UniversitySlug, line.Input.NormalizedName))),
            ImportColumns.Name);

        var targets = new ImportTargets(universityRepository, programRepository);
        var universities = await targets.FindUniversitiesAsync(
            lines.Select(line => new ImportGroup<ProgramDetails>(line.UniversitySlug, null, [line.RowNumber], [line.Input])),
            problems,
            cancellationToken);

        if (problems.Any)
        {
            return Result.Failure<ImportResponse>(problems.ToError());
        }

        int created = 0;
        foreach (var (line, university) in universities)
        {
            var details = line.Inputs[0];
            var program = await programRepository.GetByNameAsync(university.Id, details.NormalizedName, cancellationToken);
            if (program is null)
            {
                programRepository.Insert(details.ToProgram(university.Id, currentUser.UserId, dateTimeProvider.UtcNow));
                created++;
            }
            else
            {
                details.ApplyTo(program, currentUser.UserId, dateTimeProvider.UtcNow);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ImportResponse(lines.Count, created, lines.Count - created);
    }

    private static ProgramDetails ToDetails(ImportRow row) =>
        new(
            row.Text(ImportColumns.Name),
            row.Choice<DegreeLevel>(ImportColumns.DegreeLevel) ?? default,
            row.Text(ImportColumns.Field),
            row.Text(ImportColumns.Language),
            row.Int(ImportColumns.DurationMonths),
            row.Text(ImportColumns.SourceUrl),
            row.Date(ImportColumns.CheckedOn) ?? default);
}
