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
using FourPrep.Modules.Academy.Application.Enrolments.RemoveEnrolment;

internal sealed class RemoveEnrolment : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("admin/enrolments/{id:guid}", async (
            [FromServices] ISender sender,
            Guid id) =>
        {
            var result = await sender.Send(new RemoveEnrolmentCommand(id));

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
