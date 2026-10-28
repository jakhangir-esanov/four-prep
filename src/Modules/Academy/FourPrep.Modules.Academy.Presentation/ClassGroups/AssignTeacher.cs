namespace FourPrep.Modules.Academy.Presentation.ClassGroups;

using MediatR;
using FourPrep.Common.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Presentation.Results;
using FourPrep.Common.Domain.Authorization;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Modules.Academy.Presentation.ClassGroups.Requests;
using FourPrep.Modules.Academy.Application.ClassGroups.AssignTeacher;

internal sealed class AssignTeacher : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("admin/class-groups/{id:guid}/teacher", async (
            [FromServices] ISender sender,
            Guid id,
            [FromBody] AssignTeacherRequest request) =>
        {
            var result = await sender.Send(new AssignTeacherCommand(id, request.TeacherUserId, request.TeacherName ?? string.Empty));

            return result.ToResponse();
        })
        .RequireAuthorization(Policies.CourseAdmin)
        .Produces<Result>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces<Result>(StatusCodes.Status404NotFound)
        .Produces<Result>(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Academy / Admin");
    }
}
