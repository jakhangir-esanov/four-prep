namespace FourPrep.Modules.Catalogue.Application.Universities;

using FourPrep.Common.Domain.Localization;
using FourPrep.Modules.Catalogue.Application.Facts;
using FourPrep.Modules.Catalogue.Domain.Universities;

public sealed record UniversityDetails(
    string Slug,
    string Name,
    string CountryCode,
    string? City,
    string? Website,
    string? RankingSource,
    int? Ranking,
    decimal? AcceptanceRate,
    bool IsPartner,
    IReadOnlyDictionary<string, string>? Description,
    string SourceUrl,
    DateOnly CheckedOn)
{
    public string NormalizedSlug => Slug.Trim();

    internal University ToUniversity(Guid checkedByUserId, DateTime utcNow) =>
        University.Create(
            NormalizedSlug,
            Name.Trim(),
            Countries.Normalize(CountryCode),
            TextInput.TrimmedOrNull(City),
            TextInput.TrimmedOrNull(Website),
            TextInput.TrimmedOrNull(RankingSource),
            Ranking,
            AcceptanceRate,
            IsPartner,
            LocalizedTexts.Normalize(Description),
            SourceUrl.Trim(),
            CheckedOn,
            checkedByUserId,
            utcNow);

    internal void ApplyTo(University university, Guid checkedByUserId, DateTime utcNow) =>
        university.Update(
            NormalizedSlug,
            Name.Trim(),
            Countries.Normalize(CountryCode),
            TextInput.TrimmedOrNull(City),
            TextInput.TrimmedOrNull(Website),
            TextInput.TrimmedOrNull(RankingSource),
            Ranking,
            AcceptanceRate,
            IsPartner,
            LocalizedTexts.Normalize(Description),
            SourceUrl.Trim(),
            CheckedOn,
            checkedByUserId,
            utcNow);
}
