namespace FourPrep.Modules.Auth.Presentation.Authentication;

using MediatR;
using FourPrep.Common.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Presentation.Results;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Common.Presentation.RateLimiting;
using FourPrep.Modules.Auth.Presentation.Authentication.Requests;
using FourPrep.Modules.Auth.Application.Authentication.ForgotPassword;

internal sealed class ForgotPassword : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/forgot-password", async (
            [FromServices] ISender sender,
            [FromBody] ForgotPasswordRequest request) =>
        {
            var result = await sender.Send(new ForgotPasswordCommand(request.Email));

            return result.ToResponse();
        })
        .RequireRateLimiting(RateLimitPolicies.Email)
        .Produces<Result>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status429TooManyRequests)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Auth / Authentication");
    }
}
