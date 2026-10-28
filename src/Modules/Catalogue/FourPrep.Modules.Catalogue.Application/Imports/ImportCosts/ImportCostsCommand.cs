namespace FourPrep.Modules.Catalogue.Application.Imports.ImportCosts;

using FluentValidation;
using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Catalogue.Domain.Programs;
using FourPrep.Modules.Catalogue.Domain.Universities;
using FourPrep.Modules.Catalogue.Application.Abstractions.Csv;
using FourPrep.Modules.Catalogue.Application.Abstractions.Data;
using FourPrep.Modules.Catalogue.Application.Abstractions.Authentication;
using FourPrep.Modules.Catalogue.Application.Universities.ReplaceUniversityCosts;

public sealed record ImportCostsCommand(Stream File) : ICommand<ImportResponse>;

internal sealed class ImportCostsCommandHandler(
    ICsvReader csvReader,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IUniversityRepository universityRepository,
    IProgramRepository programRepository,
    IValidator<ReplaceUniversityCostsCommand> validator,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<ImportCostsCommand, ImportResponse>
{
    private static readonly ImportLayout Layout = new(
        [
            ImportColumns.UniversitySlug, ImportColumns.AcademicYear, ImportColumns.TuitionMinor,
            ImportColumns.Currency, ImportColumns.SourceUrl, ImportColumns.CheckedOn
        ],
        [ImportColumns.LivingMinor]);

    public async Task<Result<ImportResponse>> Handle(ImportCostsCommand request, CancellationToken cancellationToken)
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
            var replacement = new ReplaceUniversityCostsCommand(Guid.Empty, group.Inputs);
            problems.AddFailures(validator.Validate(replacement), group.RowNumbers, ImportColumns.AcademicYear);
        }

        var universities = await new ImportTargets(universityRepository, programRepository)
            .FindUniversitiesAsync(groups, problems, cancellationToken);

        if (problems.Any)
        {
            return Result.Failure<ImportResponse>(problems.ToError());
        }

        foreach (var (group, university) in universities)
        {
            university.ReplaceCosts(group.Inputs.Select(cost => cost.ToCost()), currentUser.UserId, dateTimeProvider.UtcNow);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ImportResponse(table.Data.Rows.Count, 0, universities.Count);
    }

    private static CostInput ToInput(ImportRow row) =>
        new(
            row.Int(ImportColumns.AcademicYear) ?? default,
            row.Long(ImportColumns.TuitionMinor) ?? default,
            row.Long(ImportColumns.LivingMinor),
            row.Text(ImportColumns.Currency),
            row.Text(ImportColumns.SourceUrl),
            row.Date(ImportColumns.CheckedOn) ?? default);
}
