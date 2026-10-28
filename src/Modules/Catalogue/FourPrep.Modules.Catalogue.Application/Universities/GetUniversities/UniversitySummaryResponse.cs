namespace FourPrep.Modules.Catalogue.Application.Universities.GetUniversities;

public sealed record UniversitySummaryResponse
{
    public Guid Id { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string CountryCode { get; init; } = string.Empty;
    public string? City { get; init; }
    public string? RankingSource { get; init; }
    public int? Ranking { get; init; }
    public decimal? AcceptanceRate { get; init; }
    public bool IsPartner { get; init; }
    public bool IsServiceDestination { get; init; }
    public DateOnly CheckedOn { get; init; }
    public bool IsStale { get; init; }
}
