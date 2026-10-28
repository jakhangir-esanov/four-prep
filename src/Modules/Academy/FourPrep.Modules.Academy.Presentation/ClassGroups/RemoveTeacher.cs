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
using FourPrep.Modules.Academy.Application.ClassGroups.RemoveTeacher;

internal sealed class RemoveTeacher : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("admin/class-groups/{id:guid}/teacher", async (
            [FromServices] ISender sender,
            Guid id) =>
        {
            var result = await sender.Send(new RemoveTeacherCommand(id));

            return result.ToResponse();
        })
        .RequireAuthorization(Policies.CourseAdmin)
        .Produces<Result>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces<Result>(StatusCodes.Status404NotFound)
        .Produces<Result>(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Academy / Admin");
    }
}
