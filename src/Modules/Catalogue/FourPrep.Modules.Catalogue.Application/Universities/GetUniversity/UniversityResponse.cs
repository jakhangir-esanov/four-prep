namespace FourPrep.Modules.Catalogue.Application.Universities.GetUniversity;

using FourPrep.Common.Domain.Enums;

public sealed record UniversityResponse
{
    public Guid Id { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string CountryCode { get; init; } = string.Empty;
    public string? City { get; init; }
    public string? Website { get; init; }
    public string? RankingSource { get; init; }
    public int? Ranking { get; init; }
    public decimal? AcceptanceRate { get; init; }
    public bool IsPartner { get; init; }
    public bool IsServiceDestination { get; init; }
    public string? VisaType { get; init; }
    public Dictionary<string, string> Description { get; init; } = [];
    public string SourceUrl { get; init; } = string.Empty;
    public DateOnly CheckedOn { get; init; }
    public bool IsStale { get; init; }
    public IReadOnlyList<ProgramResponse> Programs { get; init; } = [];
    public IReadOnlyList<CostResponse> Costs { get; init; } = [];
    public IReadOnlyList<ScholarshipResponse> Scholarships { get; init; } = [];
}

public sealed record ProgramResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public DegreeLevel DegreeLevel { get; init; }
    public string Field { get; init; } = string.Empty;
    public string Language { get; init; } = string.Empty;
    public int? DurationMonths { get; init; }
    public string SourceUrl { get; init; } = string.Empty;
    public DateOnly CheckedOn { get; init; }
    public bool IsStale { get; init; }
    public IReadOnlyList<AdmissionRequirementResponse> Requirements { get; init; } = [];
    public IReadOnlyList<DeadlineResponse> Deadlines { get; init; } = [];
}

public sealed record AdmissionRequirementResponse
{
    public Guid ProgramId { get; init; }
    public IntakeTerm IntakeTerm { get; init; }
    public int IntakeYear { get; init; }
    public Dictionary<Exam, decimal> EnglishMinimums { get; init; } = [];
    public SatPolicy? SatPolicy { get; init; }
    public int? SatMidRangeLow { get; init; }
    public int? SatMidRangeHigh { get; init; }
    public decimal? MinimumGpa { get; init; }
    public decimal? GpaScale { get; init; }
    public string[] Documents { get; init; } = [];
    public int? EssaysCount { get; init; }
    public bool? InterviewRequired { get; init; }
    public string SourceUrl { get; init; } = string.Empty;
    public DateOnly CheckedOn { get; init; }
    public bool IsStale { get; init; }
}

public sealed record DeadlineResponse
{
    public Guid ProgramId { get; init; }
    public string Round { get; init; } = string.Empty;
    public DateOnly DueDate { get; init; }
    public string SourceUrl { get; init; } = string.Empty;
    public DateOnly CheckedOn { get; init; }
    public bool IsStale { get; init; }
}

public sealed record CostResponse
{
    public int AcademicYear { get; init; }
    public long TuitionMinor { get; init; }
    public long? LivingMinor { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string SourceUrl { get; init; } = string.Empty;
    public DateOnly CheckedOn { get; init; }
    public bool IsStale { get; init; }
}

public sealed record ScholarshipResponse
{
    public string Name { get; init; } = string.Empty;
    public ScholarshipKind Kind { get; init; }
    public Dictionary<string, string> Amount { get; init; } = [];
    public Dictionary<string, string> Eligibility { get; init; } = [];
    public string SourceUrl { get; init; } = string.Empty;
    public DateOnly CheckedOn { get; init; }
    public bool IsStale { get; init; }
}
