namespace FourPrep.Modules.Admission.Application.AcademyApplications.GetAcademyApplications;

using FourPrep.Modules.Admission.Domain.AcademyApplications;

public sealed record AcademyApplicationSummaryResponse
{
    public Guid Id { get; init; }
    public Guid StudentUserId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public AcademyApplicationStatus Status { get; init; }
    public List<ApplicationInterest> Interests { get; init; } = [];
    public string MeetingLanguage { get; init; } = string.Empty;
    public DateTime SubmittedOnUtc { get; init; }
    public DateTime? ReviewedOnUtc { get; init; }
}
