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
using FourPrep.Modules.Academy.Application.ClassGroups.ChangeClassGroupStatus;

internal sealed class ChangeClassGroupStatus : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("admin/class-groups/{id:guid}/status", async (
            [FromServices] ISender sender,
            Guid id,
            [FromBody] ChangeClassGroupStatusRequest request) =>
        {
            var result = await sender.Send(new ChangeClassGroupStatusCommand(id, request.Status));

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
