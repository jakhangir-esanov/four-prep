namespace FourPrep.Modules.Catalogue.Presentation.Programs;

using MediatR;
using FourPrep.Common.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Presentation.Results;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Common.Presentation.RateLimiting;
using FourPrep.Modules.Catalogue.Application.Programs.GetMajors;

internal sealed class GetMajors : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("majors", async (
            [FromServices] ISender sender,
            [FromQuery] string? q) =>
        {
            var result = await sender.Send(new GetMajorsQuery(q));

            return result.ToResponse();
        })
        .RequireRateLimiting(RateLimitPolicies.PublicRead)
        .Produces<Result<IReadOnlyList<MajorResponse>>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status429TooManyRequests)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Catalogue / Public");
    }
}
