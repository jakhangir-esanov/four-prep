namespace FourPrep.Modules.Auth.Presentation.UserSessions;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Application.Paging;
using FourPrep.Common.Domain.Authorization;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Modules.Auth.Presentation.UserSessions.Requests;
using FourPrep.Modules.Auth.Application.UserSessions.GetUserSession;

internal sealed class GetUserSessions : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("user-sessions", async (
            [FromServices] ISender sender,
            [AsParameters] GetUserSessionsRequest request) =>
        {
            var query = new GetUserSessionsQuery(
                new PagingRequest<UserSessionResponse>(
                    request.First ?? 0,
                    request.Rows ?? 10,
                    request.SortField ?? "id",
                    request.SortOrder ?? 1),
                new UserSessionFilter(request.UserId));

            var result = await sender.Send(query);

            return result;
        })
        .RequireAuthorization(Policies.SuperAdmin)
        .Produces<PagedList<UserSessionResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Auth / UserSessions");
    }
}
