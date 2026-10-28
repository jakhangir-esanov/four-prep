namespace FourPrep.Modules.Catalogue.Presentation.Universities.Requests;

using FourPrep.Common.Domain.Enums;

internal sealed record ReplaceUniversityScholarshipsRequest
{
    public IReadOnlyList<ScholarshipRequest>? Scholarships { get; init; }
}

internal sealed record ScholarshipRequest
{
    public string? Name { get; init; }
    public ScholarshipKind Kind { get; init; }
    public Dictionary<string, string>? Amount { get; init; }
    public Dictionary<string, string>? Eligibility { get; init; }
    public string? SourceUrl { get; init; }
    public DateOnly CheckedOn { get; init; }
}
