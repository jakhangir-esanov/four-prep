namespace FourPrep.Modules.Admission.Application.AcademyApplications.GetAcademyApplications;

using FourPrep.Common.Application.Data;
using FourPrep.Modules.Admission.Domain.AcademyApplications;

public sealed record AcademyApplicationFilter(
    AcademyApplicationStatus? Status = null,
    ApplicationInterest? Interest = null,
    Guid? StudentUserId = null,
    string? Search = null)
{
    public static readonly AcademyApplicationFilter None = new();

    public string? SearchPattern => LikePatterns.Contains(Search);
}
