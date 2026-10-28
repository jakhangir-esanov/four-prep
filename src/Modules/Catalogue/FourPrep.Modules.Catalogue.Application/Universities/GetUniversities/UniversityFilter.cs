namespace FourPrep.Modules.Catalogue.Application.Universities.GetUniversities;

using FourPrep.Common.Domain.Enums;
using FourPrep.Common.Application.Data;
using FourPrep.Common.Domain.Localization;

public sealed record UniversityFilter(
    string? Search = null,
    string? Country = null,
    DegreeLevel? DegreeLevel = null,
    string? Major = null,
    long? Budget = null,
    string? Currency = null,
    int? Sat = null,
    bool? IsPartner = null,
    bool? ServiceDestinationsOnly = null)
{
    public static readonly UniversityFilter None = new();

    public string? SearchPattern => LikePatterns.Contains(Search);

    public string? MajorPattern => LikePatterns.Contains(Major);

    public string? CountryCode => Countries.NormalizeOrNull(Country);

    public string? BudgetCurrency =>
        Budget is not null && Currencies.IsValid(Currency) ? Currencies.Normalize(Currency!) : null;

    public bool HasProgramConditions => DegreeLevel is not null || MajorPattern is not null || Sat is not null;
}
