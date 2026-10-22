namespace FourPrep.Modules.Auth.Presentation.UserSessions;

using MediatR;
using FourPrep.Common.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Presentation.Results;
using FourPrep.Common.Domain.Authorization;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Modules.Auth.Application.UserSessions.GetUserSession;

internal sealed class GetUserSession : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("user-sessions/{id}", async (
            Guid id,
            [FromServices] ISender sender) =>
        {
            var query = new GetUserSessionQuery(id);

            var result = await sender.Send(query);

            return result.ToResponse();
        })
        .RequireAuthorization(Policies.SuperAdmin)
        .Produces<Result<UserSessionResponse>>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Auth / UserSessions");
    }
}
