namespace FourPrep.Modules.Storage.Presentation.StoredFiles;

using MediatR;
using FourPrep.Common.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Presentation.Results;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Modules.Storage.Application.StoredFiles.GetDownloadUrl;

internal sealed class GetDownloadUrl : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("files/{id:guid}/download-url", async (
            [FromServices] ISender sender,
            Guid id) =>
        {
            var result = await sender.Send(new GetDownloadUrlQuery(id));

            return result.ToResponse();
        })
        .RequireAuthorization()
        .Produces<Result<DownloadUrlResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces<Result>(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Storage / Files");
    }
}
