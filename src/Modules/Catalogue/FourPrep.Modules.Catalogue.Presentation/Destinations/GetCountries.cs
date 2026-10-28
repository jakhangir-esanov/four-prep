namespace FourPrep.Modules.Catalogue.Presentation.Destinations;

using MediatR;
using FourPrep.Common.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Presentation.Results;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Common.Presentation.RateLimiting;
using FourPrep.Modules.Catalogue.Application.Destinations.GetCountries;

internal sealed class GetCountries : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("countries", async ([FromServices] ISender sender) =>
        {
            var result = await sender.Send(new GetCountriesQuery());

            return result.ToResponse();
        })
        .RequireRateLimiting(RateLimitPolicies.PublicRead)
        .Produces<Result<IReadOnlyList<CountryResponse>>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status429TooManyRequests)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Catalogue / Public");
    }
}
