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
using FourPrep.Modules.Student.Application.Students.UpdateMyStudent;

internal sealed class UpdateMyStudent : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("students/me", async (
            [FromServices] ISender sender,
            [FromBody] UpdateMyStudentRequest request) =>
        {
            var command = new UpdateMyStudentCommand(
                request.FirstName,
                request.LastName,
                request.DateOfBirth,
                request.School,
                request.Grade,
                request.Country,
                request.Region,
                request.ExamGoals?.Select(goal => new ExamGoalInput(goal.Exam, goal.TargetScore)).ToList() ?? []);

            var result = await sender.Send(command);

            return result.ToResponse();
        })
        .RequireAuthorization(Policies.Student)
        .Produces<Result>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces<Result>(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Student / Students");
    }
}
