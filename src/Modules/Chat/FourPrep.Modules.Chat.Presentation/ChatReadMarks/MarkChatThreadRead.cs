namespace FourPrep.Modules.Chat.Presentation.ChatReadMarks;

using MediatR;
using FourPrep.Common.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Presentation.Results;
using FourPrep.Common.Domain.Authorization;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Modules.Chat.Domain.ChatThreads;
using FourPrep.Modules.Chat.Application.ChatReadMarks.MarkChatThreadRead;

internal sealed class MarkChatThreadRead : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("chat/students/{studentUserId:guid}/threads/{kind}/read", async (
            [FromServices] ISender sender,
            Guid studentUserId,
            ChatThreadKind kind) =>
        {
            var result = await sender.Send(new MarkChatThreadReadCommand(studentUserId, kind));

            return result.ToResponse();
        })
        .RequireAuthorization(Policies.StudentOrParent)
        .Produces<Result>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces<Result>(StatusCodes.Status403Forbidden)
        .Produces<Result>(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Chat / Family");
    }
}
