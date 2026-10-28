namespace FourPrep.Modules.Auth.Presentation.Account;

using MediatR;
using FourPrep.Common.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Domain.Authorization;
using FourPrep.Common.Presentation.Results;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Modules.Auth.Presentation.Account.Requests;
using FourPrep.Modules.Auth.Application.Account.DeleteMyAccount;

internal sealed class DeleteMyAccount : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("auth/me", async (
            [FromServices] ISender sender,
            [FromBody] DeleteMyAccountRequest? request) =>
        {
            var result = await sender.Send(new DeleteMyAccountCommand(request?.Password));

            return result.ToResponse();
        })
        .RequireAuthorization(Policies.StudentOrParent)
        .Produces<Result>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces<Result>(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Auth / Account");
    }
}
