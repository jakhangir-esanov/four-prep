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
using FourPrep.Modules.Student.Application.Students.GetStudent;

internal sealed class GetMyStudent : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("students/me", async ([FromServices] ISender sender) =>
        {
            var result = await sender.Send(new GetMyStudentQuery());

            return result.ToResponse();
        })
        .RequireAuthorization(Policies.Student)
        .Produces<Result<MyStudentResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces<Result>(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Student / Students");
    }
}
