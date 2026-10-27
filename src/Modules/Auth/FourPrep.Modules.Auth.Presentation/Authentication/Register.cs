namespace FourPrep.Modules.Auth.Presentation.Authentication;

using MediatR;
using FourPrep.Common.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Domain.Authorization;
using FourPrep.Common.Presentation.Results;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Modules.Auth.Application.Authentication;
using FourPrep.Modules.Auth.Application.Authentication.Register;
using FourPrep.Modules.Auth.Presentation.Authentication.Requests;

internal sealed class Register : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/register", async (
            [FromServices] ISender sender,
            [FromBody] RegisterRequest request) =>
        {
            var command = new RegisterCommand(
                request.Username,
                request.Email,
                request.PhoneNumber,
                request.Password,
                request.Country,
                request.TimeZone,
                request.Locale,
                Roles.Student);

            var result = await sender.Send(command);

            return result.ToResponse();
        })
        .Produces<Result<AuthTokenResponse>>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status400BadRequest)
        .Produces<Result>(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Auth / Authentication");
    }
}
