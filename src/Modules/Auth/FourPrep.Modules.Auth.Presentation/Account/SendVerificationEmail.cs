namespace FourPrep.Modules.Auth.Presentation.Account;

using MediatR;
using FourPrep.Common.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Presentation.Results;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Common.Presentation.RateLimiting;
using FourPrep.Modules.Auth.Application.Account.SendVerificationEmail;

internal sealed class SendVerificationEmail : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/me/verification-email", async ([FromServices] ISender sender) =>
        {
            var result = await sender.Send(new SendVerificationEmailCommand());

            return result.ToResponse();
        })
        .RequireAuthorization()
        .RequireRateLimiting(RateLimitPolicies.Email)
        .Produces<Result>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces<Result>(StatusCodes.Status404NotFound)
        .Produces<Result>(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status429TooManyRequests)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Auth / Account");
    }
}
