namespace FourPrep.Modules.Admission.Presentation.AcademyApplications.Requests;

using FourPrep.Modules.Admission.Domain.AcademyApplications;
using FourPrep.Modules.Admission.Application.AcademyApplications.GetAcademyApplications;

internal sealed record GetAcademyApplicationsRequest
{
    public int? First { get; init; }
    public int? Rows { get; init; }
    public string? SortField { get; init; }
    public int? SortOrder { get; init; }

    public AcademyApplicationStatus? Status { get; init; }
    public ApplicationInterest? Interest { get; init; }
    public Guid? StudentUserId { get; init; }
    public string? Search { get; init; }

    public AcademyApplicationFilter ToFilter() => new(Status, Interest, StudentUserId, Search);
}
