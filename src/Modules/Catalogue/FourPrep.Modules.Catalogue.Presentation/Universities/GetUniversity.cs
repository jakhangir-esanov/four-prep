namespace FourPrep.Modules.Catalogue.Presentation.Universities;

using MediatR;
using FourPrep.Common.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Presentation.Results;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Common.Presentation.RateLimiting;
using FourPrep.Modules.Catalogue.Application.Universities.GetUniversity;

internal sealed class GetUniversity : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("universities/{slug}", async (
            [FromServices] ISender sender,
            string slug) =>
        {
            var result = await sender.Send(new GetUniversityQuery(slug));

            return result.ToResponse();
        })
        .RequireRateLimiting(RateLimitPolicies.PublicRead)
        .Produces<Result<UniversityResponse>>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status429TooManyRequests)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Catalogue / Public");
    }
}
