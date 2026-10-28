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
using FourPrep.Modules.Academy.Application.Enrolments.GetEnrolments;

internal sealed class GetStudentEnrolments : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("enrolments", async (
            [FromServices] ISender sender,
            [FromQuery] Guid studentUserId) =>
        {
            var result = await sender.Send(new GetStudentEnrolmentsQuery(studentUserId));

            return result.ToResponse();
        })
        .RequireAuthorization(Policies.StudentOrParent)
        .Produces<Result<IReadOnlyList<EnrolmentResponse>>>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces<Result>(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Academy / Family");
    }
}
