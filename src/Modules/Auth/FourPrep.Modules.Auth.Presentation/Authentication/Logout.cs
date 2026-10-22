namespace FourPrep.Modules.Auth.Presentation.Authentication;

using MediatR;
using FourPrep.Common.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Presentation.Results;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Modules.Auth.Application.Authentication.Logout;
using FourPrep.Modules.Auth.Presentation.Authentication.Requests;

internal sealed class Logout : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/logout", async (
            [FromServices] ISender sender,
            [FromBody] LogoutRequest request) =>
        {
            var command = new LogoutCommand(request.RefreshToken);

            var result = await sender.Send(command);

            return result.ToResponse();
        })
        .RequireAuthorization()
        .Produces<Result>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces<Result>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Auth / Authentication");
    }
}
