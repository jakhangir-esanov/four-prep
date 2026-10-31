namespace FourPrep.Modules.Chat.Presentation.ChatMessages;

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
using FourPrep.Modules.Chat.Presentation.ChatMessages.Requests;
using FourPrep.Modules.Chat.Application.ChatMessages.GetChatMessages;

internal sealed class GetChatMessages : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("chat/students/{studentUserId:guid}/threads/{kind}/messages", async (
            [FromServices] ISender sender,
            Guid studentUserId,
            ChatThreadKind kind,
            [AsParameters] GetChatMessagesRequest request) =>
        {
            var result = await sender.Send(new GetChatMessagesQuery(
                studentUserId,
                kind,
                BeforeMessageId: request.Before,
                Limit: request.Limit ?? GetChatMessagesQuery.DefaultLimit));

            return result.ToResponse();
        })
        .RequireAuthorization(Policies.StudentOrParent)
        .Produces<Result<IReadOnlyList<ChatMessageResponse>>>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces<Result>(StatusCodes.Status403Forbidden)
        .Produces<Result>(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Chat / Family");
    }
}
