namespace FourPrep.Modules.Auth.Presentation.Account;

using MediatR;
using FourPrep.Common.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Presentation.Results;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Modules.Auth.Presentation.Account.Requests;
using FourPrep.Modules.Auth.Application.Account.UpdateMyAccount;

internal sealed class UpdateMyAccount : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("auth/me", async (
            [FromServices] ISender sender,
            [FromBody] UpdateMyAccountRequest request) =>
        {
            var command = new UpdateMyAccountCommand(
                request.PhoneNumber,
                request.Country,
                request.TimeZone,
                request.Locale);

            var result = await sender.Send(command);

            return result.ToResponse();
        })
        .RequireAuthorization()
        .Produces<Result>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces<Result>(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Auth / Account");
    }
}
