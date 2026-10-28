namespace FourPrep.Modules.Catalogue.Presentation.Universities.Requests;

internal sealed record ReplaceUniversityCostsRequest
{
    public IReadOnlyList<CostRequest>? Costs { get; init; }
}

internal sealed record CostRequest
{
    public int AcademicYear { get; init; }
    public long TuitionMinor { get; init; }
    public long? LivingMinor { get; init; }
    public string? Currency { get; init; }
    public string? SourceUrl { get; init; }
    public DateOnly CheckedOn { get; init; }
}
