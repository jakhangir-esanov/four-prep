namespace FourPrep.Modules.Catalogue.Presentation.Universities.Requests;

using FourPrep.Common.Domain.Enums;

internal sealed record GetUniversitiesRequest
{
    public int? First { get; init; }
    public int? Rows { get; init; }
    public string? SortField { get; init; }
    public int? SortOrder { get; init; }

    public string? Q { get; init; }
    public string? Country { get; init; }
    public DegreeLevel? Degree { get; init; }
    public string? Major { get; init; }
    public long? Budget { get; init; }
    public string? Currency { get; init; }
    public int? Sat { get; init; }
    public bool? Partner { get; init; }
    public bool? ServiceDestinationsOnly { get; init; }
}
