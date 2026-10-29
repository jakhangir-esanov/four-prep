namespace FourPrep.Modules.Storage.Presentation.StoredFiles;

using MediatR;
using FourPrep.Common.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Presentation.Results;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Modules.Storage.Application.StoredFiles.CompleteUpload;

internal sealed class CompleteUpload : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("files/{id:guid}/complete", async (
            [FromServices] ISender sender,
            Guid id) =>
        {
            var result = await sender.Send(new CompleteUploadCommand(id));

            return result.ToResponse();
        })
        .RequireAuthorization()
        .Produces<Result>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces<Result>(StatusCodes.Status404NotFound)
        .Produces<Result>(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Storage / Files");
    }
}
