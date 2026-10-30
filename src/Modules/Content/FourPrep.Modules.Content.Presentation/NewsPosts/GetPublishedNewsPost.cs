namespace FourPrep.Modules.Content.Presentation.NewsPosts;

using MediatR;
using FourPrep.Common.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Presentation.Results;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Common.Presentation.RateLimiting;
using FourPrep.Modules.Content.Application.NewsPosts.GetNewsPosts;

internal sealed class GetPublishedNewsPost : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("news/{slug}", async (
            [FromServices] ISender sender,
            string slug) =>
        {
            var result = await sender.Send(new GetPublishedNewsPostQuery(slug));

            return result.ToResponse();
        })
        .RequireRateLimiting(RateLimitPolicies.PublicRead)
        .Produces<Result<NewsPostResponse>>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status429TooManyRequests)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Content / Public");
    }
}
