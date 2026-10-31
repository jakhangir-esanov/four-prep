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
using FourPrep.Common.Presentation.RateLimiting;
using FourPrep.Modules.Chat.Domain.ChatThreads;
using FourPrep.Modules.Chat.Presentation.ChatMessages.Requests;
using FourPrep.Modules.Chat.Application.ChatMessages.SendChatMessage;

internal sealed class SendChatMessage : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("chat/students/{studentUserId:guid}/threads/{kind}/messages", async (
            [FromServices] ISender sender,
            Guid studentUserId,
            ChatThreadKind kind,
            [FromBody] SendChatMessageRequest request) =>
        {
            var result = await sender.Send(
                new SendChatMessageCommand(studentUserId, kind, request.Body));

            return result.ToResponse();
        })
        .RequireAuthorization(Policies.StudentOrParent)
        .RequireRateLimiting(RateLimitPolicies.Messaging)
        .Produces<Result<Guid>>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces<Result>(StatusCodes.Status403Forbidden)
        .Produces<Result>(StatusCodes.Status404NotFound)
        .Produces<Result>(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status429TooManyRequests)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Chat / Family");
    }
}
