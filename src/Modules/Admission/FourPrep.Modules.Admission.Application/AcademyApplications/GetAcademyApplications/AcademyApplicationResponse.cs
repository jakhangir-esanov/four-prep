namespace FourPrep.Modules.Admission.Application.AcademyApplications.GetAcademyApplications;

using FourPrep.Modules.Admission.Domain.AcademyApplications;

public sealed record AcademyApplicationResponse
{
    public Guid Id { get; init; }
    public Guid StudentUserId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public AcademyApplicationStatus Status { get; init; }
    public List<ApplicationInterest> Interests { get; init; } = [];
    public IReadOnlyList<ReportedScoreResponse> Scores { get; init; } = [];
    public List<ContactChannel> ContactChannels { get; init; } = [];
    public string? PhoneNumber { get; init; }
    public string? TelegramUsername { get; init; }
    public string MeetingLanguage { get; init; } = string.Empty;
    public string? Question { get; init; }
    public bool ParentConsentConfirmed { get; init; }
    public DateTime SubmittedOnUtc { get; init; }
    public Guid? ReviewedByUserId { get; init; }
    public DateTime? ReviewedOnUtc { get; init; }
    public string? DeclineReason { get; init; }
    public DateTime? WithdrawnOnUtc { get; init; }

    internal AcademyApplicationResponse ForFamily() => this with { ReviewedByUserId = null };
}
