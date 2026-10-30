namespace FourPrep.Modules.Content.Presentation.NewsPosts;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Application.Paging;
using FourPrep.Common.Domain.Authorization;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Modules.Content.Presentation.NewsPosts.Requests;
using FourPrep.Modules.Content.Application.NewsPosts.GetNewsPosts;

internal sealed class GetNewsPosts : IEndpoint
{
    private const int DefaultRows = 20;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("admin/news", async (
            [FromServices] ISender sender,
            [AsParameters] GetNewsPostsRequest request) =>
        {
            var query = new GetNewsPostsQuery(
                new PagingRequest<NewsPostSummaryResponse>(
                    request.First ?? 0,
                    request.Rows ?? DefaultRows,
                    request.SortField ?? "updated_on_utc",
                    request.SortOrder ?? -1),
                new NewsPostFilter(request.Status, request.Tag, request.Locale, request.Search));

            var result = await sender.Send(query);

            return result;
        })
        .RequireAuthorization(Policies.ContentAdmin)
        .Produces<PagedList<NewsPostSummaryResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Content / Admin");
    }
}
