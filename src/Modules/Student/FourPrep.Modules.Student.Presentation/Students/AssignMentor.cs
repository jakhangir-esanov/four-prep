namespace FourPrep.Modules.Student.Presentation.Students;

using MediatR;
using FourPrep.Common.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Domain.Authorization;
using FourPrep.Common.Presentation.Results;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Modules.Student.Presentation.Students.Requests;
using FourPrep.Modules.Student.Application.Students.AssignMentor;

internal sealed class AssignMentor : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("students/{id:guid}/mentor", async (
            [FromServices] ISender sender,
            Guid id,
            [FromBody] AssignMentorRequest request) =>
        {
            var result = await sender.Send(new AssignMentorCommand(id, request.MentorUserId));

            return result.ToResponse();
        })
        .RequireAuthorization(Policies.MentorAdmin)
        .Produces<Result>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces<Result>(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Student / Admin");
    }
}
