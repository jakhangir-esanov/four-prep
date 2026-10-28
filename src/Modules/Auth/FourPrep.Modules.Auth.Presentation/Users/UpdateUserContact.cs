namespace FourPrep.Modules.Auth.Presentation.Users;

using MediatR;
using FourPrep.Common.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Domain.Authorization;
using FourPrep.Common.Presentation.Results;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Modules.Auth.Presentation.Users.Requests;
using FourPrep.Modules.Auth.Application.Users.UpdateUserContact;

internal sealed class UpdateUserContact : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("admin/users/{id:guid}/contact", async (
            [FromServices] ISender sender,
            Guid id,
            [FromBody] UpdateUserContactRequest request) =>
        {
            var result = await sender.Send(new UpdateUserContactCommand(id, request.Email, request.PhoneNumber));

            return result.ToResponse();
        })
        .RequireAuthorization(Policies.SuperAdmin)
        .Produces<Result>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces<Result>(StatusCodes.Status404NotFound)
        .Produces<Result>(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Auth / Users");
    }
}
