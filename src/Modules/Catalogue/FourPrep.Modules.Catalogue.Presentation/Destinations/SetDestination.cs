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
using FourPrep.Modules.Catalogue.Presentation.Destinations.Requests;
using FourPrep.Modules.Catalogue.Application.Destinations.SetDestination;

internal sealed class SetDestination : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("admin/destinations/{countryCode}", async (
            [FromServices] ISender sender,
            string countryCode,
            [FromBody] SetDestinationRequest request) =>
        {
            var result = await sender.Send(new SetDestinationCommand(countryCode, request.VisaType ?? string.Empty));

            return result.ToResponse();
        })
        .RequireAuthorization(Policies.ContentAdmin)
        .Produces<Result>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Catalogue / Admin");
    }
}
