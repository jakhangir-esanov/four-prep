namespace FourPrep.Modules.Auth.Presentation.Users;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Application.Paging;
using FourPrep.Common.Domain.Authorization;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Modules.Auth.Application.Users.GetUser;
using FourPrep.Modules.Auth.Presentation.Users.Requests;

internal sealed class GetUsers : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("admin/users", async (
            [FromServices] ISender sender,
            [AsParameters] GetUsersRequest request) =>
        {
            var query = new GetUsersQuery(
                new PagingRequest<UserResponse>(
                    request.First ?? 0,
                    request.Rows ?? 10,
                    request.SortField ?? "user_name",
                    request.SortOrder ?? 1),
                new UserFilter(
                    request.Search,
                    request.IsActive,
                    request.Role));

            var result = await sender.Send(query);

            return result;
        })
        .RequireAuthorization(Policies.SuperAdmin)
        .Produces<PagedList<UserResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Auth / Users");
    }
}
