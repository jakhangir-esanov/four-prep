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
using FourPrep.Modules.Chat.Application.ChatThreads;
using FourPrep.Modules.Chat.Presentation.ChatMessages.Requests;
using FourPrep.Modules.Chat.Application.ChatMessages.SendChatMessage;

internal sealed class SendMenteeChatMessage : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("staff/chat/students/{studentUserId:guid}/messages", async (
            [FromServices] ISender sender,
            Guid studentUserId,
            [FromBody] SendChatMessageRequest request) =>
        {
            var result = await sender.Send(
                new SendChatMessageCommand(studentUserId, ChatThreadKind.Mentor, request.Body, ChatAudience.Mentor));

            return result.ToResponse();
        })
        .RequireAuthorization(Policies.Mentor)
        .RequireRateLimiting(RateLimitPolicies.Messaging)
        .Produces<Result<Guid>>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces<Result>(StatusCodes.Status403Forbidden)
        .Produces<Result>(StatusCodes.Status404NotFound)
        .Produces<Result>(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status429TooManyRequests)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Chat / Staff");
    }
}
