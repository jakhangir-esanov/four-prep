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
using FourPrep.Modules.Catalogue.Application.Programs.DeleteProgram;

internal sealed class DeleteProgram : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("admin/programs/{id:guid}", async (
            [FromServices] ISender sender,
            Guid id) =>
        {
            var result = await sender.Send(new DeleteProgramCommand(id));

            return result.ToResponse();
        })
        .RequireAuthorization(Policies.ContentAdmin)
        .Produces<Result>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces<Result>(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Catalogue / Admin");
    }
}
