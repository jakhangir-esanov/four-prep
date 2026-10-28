namespace FourPrep.Modules.Academy.Presentation.Enrolments;

using MediatR;
using FourPrep.Common.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Presentation.Results;
using FourPrep.Common.Domain.Authorization;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Modules.Academy.Presentation.Enrolments.Requests;
using FourPrep.Modules.Academy.Application.Enrolments.EnrolStudent;

internal sealed class EnrolStudent : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("class-groups/{id:guid}/enrolments", async (
            [FromServices] ISender sender,
            Guid id,
            [FromBody] EnrolStudentRequest request) =>
        {
            var result = await sender.Send(new EnrolStudentCommand(id, request.StudentUserId));

            return result.ToResponse();
        })
        .RequireAuthorization(Policies.StudentOrParent)
        .Produces<Result<EnrolmentCreatedResponse>>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces<Result>(StatusCodes.Status403Forbidden)
        .Produces<Result>(StatusCodes.Status404NotFound)
        .Produces<Result>(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Academy / Family");
    }
}
