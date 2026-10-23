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
using FourPrep.Modules.Student.Application.StudentParents.UnlinkStudent;

internal sealed class UnlinkStudent : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("parents/me/students/{studentId:guid}", async (
            [FromServices] ISender sender,
            Guid studentId) =>
        {
            var result = await sender.Send(new UnlinkStudentCommand(studentId));

            return result.ToResponse();
        })
        .RequireAuthorization(Policies.Parent)
        .Produces<Result>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces<Result>(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Student / Parents");
    }
}
