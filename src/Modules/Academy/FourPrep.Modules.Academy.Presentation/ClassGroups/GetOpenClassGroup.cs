namespace FourPrep.Modules.Academy.Presentation.ClassGroups;

using MediatR;
using FourPrep.Common.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Presentation.Results;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Common.Presentation.RateLimiting;
using FourPrep.Modules.Academy.Application.ClassGroups.GetClassGroups;

internal sealed class GetOpenClassGroup : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("class-groups/{id:guid}", async (
            [FromServices] ISender sender,
            Guid id) =>
        {
            var result = await sender.Send(new GetOpenClassGroupQuery(id));

            return result.ToResponse();
        })
        .RequireRateLimiting(RateLimitPolicies.PublicRead)
        .Produces<Result<ClassGroupResponse>>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status429TooManyRequests)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Academy / Public");
    }
}
