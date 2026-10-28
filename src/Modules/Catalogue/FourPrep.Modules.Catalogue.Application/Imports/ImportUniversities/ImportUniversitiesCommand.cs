namespace FourPrep.Modules.Catalogue.Application.Imports.ImportUniversities;

using FluentValidation;
using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Catalogue.Domain.Universities;
using FourPrep.Modules.Catalogue.Application.Universities;
using FourPrep.Modules.Catalogue.Application.Abstractions.Csv;
using FourPrep.Modules.Catalogue.Application.Abstractions.Data;
using FourPrep.Modules.Catalogue.Application.Abstractions.Authentication;

public sealed record ImportUniversitiesCommand(Stream File) : ICommand<ImportResponse>;

internal sealed class ImportUniversitiesCommandHandler(
    ICsvReader csvReader,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IUniversityRepository repository,
    IValidator<UniversityDetails> validator,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<ImportUniversitiesCommand, ImportResponse>
{
    private static readonly ImportLayout Layout = new(
        [ImportColumns.Slug, ImportColumns.Name, ImportColumns.CountryCode, ImportColumns.SourceUrl, ImportColumns.CheckedOn],
        [ImportColumns.City, ImportColumns.Website, ImportColumns.RankingSource, ImportColumns.Ranking, ImportColumns.AcceptanceRate, ImportColumns.IsPartner],
        [ImportColumns.Description]);

    public async Task<Result<ImportResponse>> Handle(ImportUniversitiesCommand request, CancellationToken cancellationToken)
    {
        var table = ImportFile.Read(csvReader, request.File, Layout);
        if (table.IsFailure)
        {
            return Result.Failure<ImportResponse>(table.Error);
        }

        var problems = new ImportProblems();
        var rows = table.Data.Rows
            .Select(csvRow => new ImportRow(csvRow, problems))
            .Select(row => (row.Number, Details: ToDetails(row)))
            .ToList();

        foreach (var (rowNumber, details) in rows)
        {
            problems.AddFailures(validator.Validate(details), rowNumber);
        }

        problems.AddDuplicates(rows.Select(row => (row.Number, row.Details.NormalizedSlug)), ImportColumns.Slug);

        if (problems.Any)
        {
            return Result.Failure<ImportResponse>(problems.ToError());
        }

        int created = 0;
        foreach (var (_, details) in rows)
        {
            var university = await repository.GetBySlugAsync(details.NormalizedSlug, cancellationToken);
            if (university is null)
            {
                repository.Insert(details.ToUniversity(currentUser.UserId, dateTimeProvider.UtcNow));
                created++;
            }
            else
            {
                details.ApplyTo(university, currentUser.UserId, dateTimeProvider.UtcNow);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ImportResponse(rows.Count, created, rows.Count - created);
    }

    private static UniversityDetails ToDetails(ImportRow row) =>
        new(
            row.Text(ImportColumns.Slug),
            row.Text(ImportColumns.Name),
            row.Text(ImportColumns.CountryCode),
            row.OptionalText(ImportColumns.City),
            row.OptionalText(ImportColumns.Website),
            row.OptionalText(ImportColumns.RankingSource),
            row.Int(ImportColumns.Ranking),
            row.Decimal(ImportColumns.AcceptanceRate),
            row.Bool(ImportColumns.IsPartner) ?? false,
            row.Localized(ImportColumns.Description),
            row.Text(ImportColumns.SourceUrl),
            row.Date(ImportColumns.CheckedOn) ?? default);
}
