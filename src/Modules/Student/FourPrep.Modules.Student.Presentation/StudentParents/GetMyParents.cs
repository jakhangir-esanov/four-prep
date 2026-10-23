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
using FourPrep.Modules.Student.Application.StudentParents.GetMyParents;

internal sealed class GetMyParents : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("students/me/parents", async ([FromServices] ISender sender) =>
        {
            var result = await sender.Send(new GetMyParentsQuery());

            return result.ToResponse();
        })
        .RequireAuthorization(Policies.Student)
        .Produces<Result<IReadOnlyList<ParentResponse>>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Student / Students");
    }
}
