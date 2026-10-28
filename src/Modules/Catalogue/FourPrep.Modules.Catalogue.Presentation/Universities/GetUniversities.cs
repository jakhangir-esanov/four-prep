namespace FourPrep.Modules.Catalogue.Presentation.Universities;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Application.Paging;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Common.Presentation.RateLimiting;
using FourPrep.Modules.Catalogue.Presentation.Universities.Requests;
using FourPrep.Modules.Catalogue.Application.Universities.GetUniversities;

internal sealed class GetUniversities : IEndpoint
{
    private const int DefaultRows = 20;
    private const int MaxPublicRows = 100;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("universities", async (
            [FromServices] ISender sender,
            [AsParameters] GetUniversitiesRequest request) =>
        {
            var query = new GetUniversitiesQuery(
                new PagingRequest<UniversitySummaryResponse>(
                    request.First ?? 0,
                    Math.Min(request.Rows ?? DefaultRows, MaxPublicRows),
                    request.SortField ?? "name",
                    request.SortOrder ?? 1),
                new UniversityFilter(
                    request.Q,
                    request.Country,
                    request.Degree,
                    request.Major,
                    request.Budget,
                    request.Currency,
                    request.Sat,
                    request.Partner,
                    request.ServiceDestinationsOnly));

            var result = await sender.Send(query);

            return result;
        })
        .RequireRateLimiting(RateLimitPolicies.PublicRead)
        .Produces<PagedList<UniversitySummaryResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status429TooManyRequests)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Catalogue / Public");
    }
}
