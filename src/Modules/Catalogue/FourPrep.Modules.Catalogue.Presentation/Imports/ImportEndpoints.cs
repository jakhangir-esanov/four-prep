namespace FourPrep.Modules.Catalogue.Presentation.Imports;

using MediatR;
using FourPrep.Common.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Domain.Authorization;
using FourPrep.Common.Presentation.Results;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Catalogue.Application.Imports;

internal static class ImportEndpoints
{
    private const long MaxFileBytes = 5 * 1024 * 1024;

    public static RouteHandlerBuilder MapImport(
        this IEndpointRouteBuilder app,
        string pattern,
        Func<Stream, ICommand<ImportResponse>> createCommand) =>
        app.MapPost(pattern, async (
            [FromServices] ISender sender,
            IFormFile file) =>
        {
            await using var stream = file.OpenReadStream();

            var result = await sender.Send(createCommand(stream));

            return result.ToResponse();
        })
        .DisableAntiforgery()
        .WithMetadata(new RequestSizeLimitAttribute(MaxFileBytes))
        .RequireAuthorization(Policies.ContentAdmin)
        .Produces<Result<ImportResponse>>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status413PayloadTooLarge)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Catalogue / Admin");
}
