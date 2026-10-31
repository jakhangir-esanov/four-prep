namespace FourPrep.Modules.Admission.Presentation.AcademyApplications;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Application.Paging;
using FourPrep.Common.Domain.Authorization;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Modules.Admission.Presentation.AcademyApplications.Requests;
using FourPrep.Modules.Admission.Application.AcademyApplications.GetAcademyApplications;

internal sealed class GetAcademyApplications : IEndpoint
{
    private const int DefaultRows = 20;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("admin/academy-applications", async (
            [FromServices] ISender sender,
            [AsParameters] GetAcademyApplicationsRequest request) =>
        {
            var query = new GetAcademyApplicationsQuery(
                new PagingRequest<AcademyApplicationSummaryResponse>(
                    request.First ?? 0,
                    request.Rows ?? DefaultRows,
                    request.SortField ?? "submitted_on_utc",
                    request.SortOrder ?? -1),
                request.ToFilter());

            var result = await sender.Send(query);

            return result;
        })
        .RequireAuthorization(Policies.MentorAdmin)
        .Produces<PagedList<AcademyApplicationSummaryResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Admission / Admin");
    }
}
