namespace FourPrep.Modules.Catalogue.Presentation.Destinations;

using MediatR;
using FourPrep.Common.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Domain.Authorization;
using FourPrep.Common.Presentation.Results;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Modules.Catalogue.Application.Destinations.RemoveDestination;

internal sealed class RemoveDestination : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("admin/destinations/{countryCode}", async (
            [FromServices] ISender sender,
            string countryCode) =>
        {
            var result = await sender.Send(new RemoveDestinationCommand(countryCode));

            return result.ToResponse();
        })
        .RequireAuthorization(Policies.ContentAdmin)
        .Produces<Result>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces<Result>(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Catalogue / Admin");
    }
}
