namespace FourPrep.Modules.Auth.Presentation.Account;

using MediatR;
using FourPrep.Common.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Presentation.Results;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Modules.Auth.Application.Authentication;
using FourPrep.Modules.Auth.Presentation.Account.Requests;
using FourPrep.Modules.Auth.Application.Account.ChangeMyPassword;

internal sealed class ChangeMyPassword : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("auth/me/password", async (
            [FromServices] ISender sender,
            [FromBody] ChangeMyPasswordRequest request) =>
        {
            var result = await sender.Send(new ChangeMyPasswordCommand(request.CurrentPassword, request.NewPassword));

            return result.ToResponse();
        })
        .RequireAuthorization()
        .Produces<Result<AuthTokenResponse>>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces<Result>(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Auth / Account");
    }
}
