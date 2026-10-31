namespace FourPrep.Modules.Admission.Presentation.AcademyApplications.Requests;

internal sealed record DeclineAcademyApplicationRequest
{
    public string Reason { get; init; } = string.Empty;
}
