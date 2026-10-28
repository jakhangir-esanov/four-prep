namespace FourPrep.Modules.Academy.Presentation.ClassGroups;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Application.Paging;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Common.Presentation.RateLimiting;
using FourPrep.Modules.Academy.Presentation.ClassGroups.Requests;
using FourPrep.Modules.Academy.Application.ClassGroups.GetClassGroups;

internal sealed class GetOpenClassGroups : IEndpoint
{
    private const int DefaultRows = 20;
    private const int MaxPublicRows = 100;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("class-groups", async (
            [FromServices] ISender sender,
            [AsParameters] GetOpenClassGroupsRequest request) =>
        {
            var query = new GetOpenClassGroupsQuery(
                new PagingRequest<ClassGroupResponse>(
                    request.First ?? 0,
                    Math.Min(request.Rows ?? DefaultRows, MaxPublicRows),
                    request.SortField ?? "starts_on",
                    request.SortOrder ?? 1),
                new ClassGroupFilter(request.Course, request.Mode));

            var result = await sender.Send(query);

            return result;
        })
        .RequireRateLimiting(RateLimitPolicies.PublicRead)
        .Produces<PagedList<ClassGroupResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status429TooManyRequests)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Academy / Public");
    }
}
