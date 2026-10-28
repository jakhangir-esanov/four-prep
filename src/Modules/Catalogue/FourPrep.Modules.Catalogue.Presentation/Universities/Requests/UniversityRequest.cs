namespace FourPrep.Modules.Catalogue.Presentation.Universities.Requests;

using FourPrep.Modules.Catalogue.Application.Universities;

internal sealed record UniversityRequest
{
    public string? Slug { get; init; }
    public string? Name { get; init; }
    public string? CountryCode { get; init; }
    public string? City { get; init; }
    public string? Website { get; init; }
    public string? RankingSource { get; init; }
    public int? Ranking { get; init; }
    public decimal? AcceptanceRate { get; init; }
    public bool IsPartner { get; init; }
    public Dictionary<string, string>? Description { get; init; }
    public string? SourceUrl { get; init; }
    public DateOnly CheckedOn { get; init; }

    public UniversityDetails ToDetails() =>
        new(
            Slug ?? string.Empty,
            Name ?? string.Empty,
            CountryCode ?? string.Empty,
            City,
            Website,
            RankingSource,
            Ranking,
            AcceptanceRate,
            IsPartner,
            Description,
            SourceUrl ?? string.Empty,
            CheckedOn);
}
