namespace FourPrep.Modules.Auth.Presentation.Authentication;

using MediatR;
using FourPrep.Common.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Presentation.Results;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Modules.Auth.Application.Authentication;
using FourPrep.Modules.Auth.Presentation.Authentication.Requests;
using FourPrep.Modules.Auth.Application.Authentication.GoogleLogin;

internal sealed class GoogleLogin : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/google", async (
            [FromServices] ISender sender,
            [FromBody] GoogleLoginRequest request) =>
        {
            var command = new GoogleLoginCommand(
                request.IdToken,
                request.Role,
                request.Country,
                request.TimeZone,
                request.Locale);

            var result = await sender.Send(command);

            return result.ToResponse();
        })
        .Produces<Result<AuthTokenResponse>>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status400BadRequest)
        .Produces<Result>(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Auth / Authentication");
    }
}
