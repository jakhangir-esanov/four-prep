namespace FourPrep.Modules.Catalogue.Presentation.Programs;

using MediatR;
using FourPrep.Common.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Domain.Authorization;
using FourPrep.Common.Presentation.Results;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Modules.Catalogue.Presentation.Programs.Requests;
using FourPrep.Modules.Catalogue.Application.Programs.CreateProgram;

internal sealed class CreateProgram : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("admin/universities/{universityId:guid}/programs", async (
            [FromServices] ISender sender,
            Guid universityId,
            [FromBody] ProgramRequest request) =>
        {
            var result = await sender.Send(new CreateProgramCommand(universityId, request.ToDetails()));

            return result.ToResponse();
        })
        .RequireAuthorization(Policies.ContentAdmin)
        .Produces<Result<Guid>>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces<Result>(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Catalogue / Admin");
    }
}
