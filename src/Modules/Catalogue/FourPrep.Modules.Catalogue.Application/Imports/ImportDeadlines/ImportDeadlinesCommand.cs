namespace FourPrep.Modules.Catalogue.Application.Imports.ImportDeadlines;

using FluentValidation;
using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Catalogue.Domain.Programs;
using FourPrep.Modules.Catalogue.Domain.Universities;
using FourPrep.Modules.Catalogue.Application.Abstractions.Csv;
using FourPrep.Modules.Catalogue.Application.Abstractions.Data;
using FourPrep.Modules.Catalogue.Application.Abstractions.Authentication;
using FourPrep.Modules.Catalogue.Application.Programs.ReplaceProgramDeadlines;

public sealed record ImportDeadlinesCommand(Stream File) : ICommand<ImportResponse>;

internal sealed class ImportDeadlinesCommandHandler(
    ICsvReader csvReader,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IUniversityRepository universityRepository,
    IProgramRepository programRepository,
    IValidator<ReplaceProgramDeadlinesCommand> validator,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<ImportDeadlinesCommand, ImportResponse>
{
    private static readonly ImportLayout Layout = new(
        [
            ImportColumns.UniversitySlug, ImportColumns.ProgramName, ImportColumns.Round,
            ImportColumns.DueDate, ImportColumns.SourceUrl, ImportColumns.CheckedOn
        ],
        []);

    public async Task<Result<ImportResponse>> Handle(ImportDeadlinesCommand request, CancellationToken cancellationToken)
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
            var replacement = new ReplaceProgramDeadlinesCommand(Guid.Empty, group.Inputs);
            problems.AddFailures(validator.Validate(replacement), group.RowNumbers, ImportColumns.Round);
        }

        var programs = await new ImportTargets(universityRepository, programRepository)
            .FindProgramsAsync(groups, problems, cancellationToken);

        if (problems.Any)
        {
            return Result.Failure<ImportResponse>(problems.ToError());
        }

        foreach (var (group, program) in programs)
        {
            program.ReplaceDeadlines(group.Inputs.Select(deadline => deadline.ToDeadline()), currentUser.UserId, dateTimeProvider.UtcNow);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ImportResponse(table.Data.Rows.Count, 0, programs.Count);
    }

    private static DeadlineInput ToInput(ImportRow row) =>
        new(
            row.Text(ImportColumns.Round),
            row.Date(ImportColumns.DueDate) ?? default,
            row.Text(ImportColumns.SourceUrl),
            row.Date(ImportColumns.CheckedOn) ?? default);
}
