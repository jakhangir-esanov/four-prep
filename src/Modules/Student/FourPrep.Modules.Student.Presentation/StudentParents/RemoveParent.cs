namespace FourPrep.Modules.Student.Presentation.StudentParents;

using MediatR;
using FourPrep.Common.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Domain.Authorization;
using FourPrep.Common.Presentation.Results;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Modules.Student.Application.StudentParents.RemoveParent;

internal sealed class RemoveParent : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("students/me/parents/{parentUserId:guid}", async (
            [FromServices] ISender sender,
            Guid parentUserId) =>
        {
            var result = await sender.Send(new RemoveParentCommand(parentUserId));

            return result.ToResponse();
        })
        .RequireAuthorization(Policies.Student)
        .Produces<Result>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces<Result>(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Student / Students");
    }
}
