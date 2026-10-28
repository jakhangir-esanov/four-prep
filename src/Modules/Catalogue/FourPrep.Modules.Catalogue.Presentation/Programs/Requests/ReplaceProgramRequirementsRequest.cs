namespace FourPrep.Modules.Catalogue.Presentation.Programs.Requests;

using FourPrep.Common.Domain.Enums;

internal sealed record ReplaceProgramRequirementsRequest
{
    public IReadOnlyList<AdmissionRequirementRequest>? Requirements { get; init; }
}

internal sealed record AdmissionRequirementRequest
{
    public IntakeTerm IntakeTerm { get; init; }
    public int IntakeYear { get; init; }
    public Dictionary<Exam, decimal>? EnglishMinimums { get; init; }
    public SatPolicy? SatPolicy { get; init; }
    public int? SatMidRangeLow { get; init; }
    public int? SatMidRangeHigh { get; init; }
    public decimal? MinimumGpa { get; init; }
    public decimal? GpaScale { get; init; }
    public IReadOnlyList<string>? Documents { get; init; }
    public int? EssaysCount { get; init; }
    public bool? InterviewRequired { get; init; }
    public string? SourceUrl { get; init; }
    public DateOnly CheckedOn { get; init; }
}
