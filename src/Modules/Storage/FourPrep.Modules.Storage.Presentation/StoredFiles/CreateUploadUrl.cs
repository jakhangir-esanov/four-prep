namespace FourPrep.Modules.Storage.Presentation.StoredFiles;

using MediatR;
using FourPrep.Common.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Presentation.Results;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Common.Presentation.RateLimiting;
using FourPrep.Modules.Storage.Presentation.StoredFiles.Requests;
using FourPrep.Modules.Storage.Application.StoredFiles.CreateUploadUrl;

internal sealed class CreateUploadUrl : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("files/upload-url", async (
            [FromServices] ISender sender,
            [FromBody] CreateUploadUrlRequest request) =>
        {
            var result = await sender.Send(new CreateUploadUrlCommand(
                request.FileName ?? string.Empty,
                request.ContentType ?? string.Empty,
                request.SizeBytes));

            return result.ToResponse();
        })
        .RequireAuthorization()
        .RequireRateLimiting(RateLimitPolicies.FileUpload)
        .Produces<Result<UploadUrlResponse>>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces<Result>(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status429TooManyRequests)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Storage / Files");
    }
}
