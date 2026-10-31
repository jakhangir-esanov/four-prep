namespace FourPrep.Modules.Admission.Presentation.AcademyApplications.Requests;

using FourPrep.Modules.Admission.Domain.AcademyApplications;
using FourPrep.Modules.Admission.Application.AcademyApplications.SubmitAcademyApplication;

internal sealed record SubmitAcademyApplicationRequest
{
    public IReadOnlyList<ApplicationInterest> Interests { get; init; } = [];
    public IReadOnlyList<ReportedScoreInput> Scores { get; init; } = [];
    public IReadOnlyList<ContactChannel> ContactChannels { get; init; } = [];
    public string? PhoneNumber { get; init; }
    public string? TelegramUsername { get; init; }
    public string MeetingLanguage { get; init; } = string.Empty;
    public string? Question { get; init; }
    public bool ParentConsentConfirmed { get; init; }
}
