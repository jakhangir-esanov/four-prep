namespace FourPrep.Modules.Catalogue.Application.Imports.ImportScholarships;

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
using FourPrep.Modules.Catalogue.Application.Universities.ReplaceUniversityScholarships;

public sealed record ImportScholarshipsCommand(Stream File) : ICommand<ImportResponse>;

internal sealed class ImportScholarshipsCommandHandler(
    ICsvReader csvReader,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IUniversityRepository universityRepository,
    IProgramRepository programRepository,
    IValidator<ReplaceUniversityScholarshipsCommand> validator,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<ImportScholarshipsCommand, ImportResponse>
{
    private static readonly ImportLayout Layout = new(
        [ImportColumns.UniversitySlug, ImportColumns.Name, ImportColumns.Kind, ImportColumns.SourceUrl, ImportColumns.CheckedOn],
        [],
        [ImportColumns.Amount, ImportColumns.Eligibility]);

    public async Task<Result<ImportResponse>> Handle(ImportScholarshipsCommand request, CancellationToken cancellationToken)
    {
        var table = ImportFile.Read(csvReader, request.File, Layout);
        if (table.IsFailure)
        {
            return Result.Failure<ImportResponse>(table.Error);
        }

        var problems = new ImportProblems();
        var groups = ImportLines.Group(ImportLines.ForUniversities(table.Data, problems, ToInput));

        foreach (var group in groups)
        {
            var replacement = new ReplaceUniversityScholarshipsCommand(Guid.Empty, group.Inputs);
            problems.AddFailures(validator.Validate(replacement), group.RowNumbers, ImportColumns.UniversitySlug);
        }

        var universities = await new ImportTargets(universityRepository, programRepository)
            .FindUniversitiesAsync(groups, problems, cancellationToken);

        if (problems.Any)
        {
            return Result.Failure<ImportResponse>(problems.ToError());
        }

        foreach (var (group, university) in universities)
        {
            university.ReplaceScholarships(
                group.Inputs.Select(scholarship => scholarship.ToScholarship()),
                currentUser.UserId,
                dateTimeProvider.UtcNow);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ImportResponse(table.Data.Rows.Count, 0, universities.Count);
    }

    private static ScholarshipInput ToInput(ImportRow row) =>
        new(
            row.Text(ImportColumns.Name),
            row.Choice<ScholarshipKind>(ImportColumns.Kind) ?? default,
            row.Localized(ImportColumns.Amount),
            row.Localized(ImportColumns.Eligibility),
            row.Text(ImportColumns.SourceUrl),
            row.Date(ImportColumns.CheckedOn) ?? default);
}
