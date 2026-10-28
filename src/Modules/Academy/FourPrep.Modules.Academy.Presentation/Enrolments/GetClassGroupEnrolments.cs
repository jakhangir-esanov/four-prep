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

internal sealed class GetClassGroupEnrolments : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("admin/class-groups/{id:guid}/enrolments", async (
            [FromServices] ISender sender,
            Guid id) =>
        {
            var result = await sender.Send(new GetClassGroupEnrolmentsQuery(id));

            return result.ToResponse();
        })
        .RequireAuthorization(Policies.CourseAdmin)
        .Produces<Result<IReadOnlyList<EnrolmentResponse>>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Academy / Admin");
    }
}
