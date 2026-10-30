namespace FourPrep.Modules.Content.Presentation.NewsPosts;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Application.Paging;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Common.Presentation.RateLimiting;
using FourPrep.Modules.Content.Presentation.NewsPosts.Requests;
using FourPrep.Modules.Content.Application.NewsPosts.GetNewsPosts;

internal sealed class GetPublishedNewsPosts : IEndpoint
{
    private const int DefaultRows = 20;
    private const int MaxPublicRows = 100;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("news", async (
            [FromServices] ISender sender,
            [AsParameters] GetPublishedNewsPostsRequest request) =>
        {
            var query = new GetPublishedNewsPostsQuery(
                new PagingRequest<NewsPostSummaryResponse>(
                    request.First ?? 0,
                    Math.Min(request.Rows ?? DefaultRows, MaxPublicRows),
                    request.SortField ?? "published_on_utc",
                    request.SortOrder ?? -1),
                new NewsPostFilter(Tag: request.Tag, Locale: request.Locale));

            var result = await sender.Send(query);

            return result;
        })
        .RequireRateLimiting(RateLimitPolicies.PublicRead)
        .Produces<PagedList<NewsPostSummaryResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status429TooManyRequests)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Content / Public");
    }
}
