namespace FourPrep.Modules.Academy.Presentation.ClassGroups;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Application.Paging;
using FourPrep.Common.Domain.Authorization;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Modules.Academy.Presentation.ClassGroups.Requests;
using FourPrep.Modules.Academy.Application.ClassGroups.GetClassGroups;

internal sealed class GetClassGroups : IEndpoint
{
    private const int DefaultRows = 20;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("admin/class-groups", async (
            [FromServices] ISender sender,
            [AsParameters] GetClassGroupsRequest request) =>
        {
            var query = new GetClassGroupsQuery(
                new PagingRequest<ClassGroupResponse>(
                    request.First ?? 0,
                    request.Rows ?? DefaultRows,
                    request.SortField ?? "starts_on",
                    request.SortOrder ?? -1),
                new ClassGroupFilter(
                    request.Course,
                    request.Mode,
                    request.Status,
                    request.TeacherUserId,
                    request.Search));

            var result = await sender.Send(query);

            return result;
        })
        .RequireAuthorization(Policies.CourseAdmin)
        .Produces<PagedList<ClassGroupResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Academy / Admin");
    }
}
