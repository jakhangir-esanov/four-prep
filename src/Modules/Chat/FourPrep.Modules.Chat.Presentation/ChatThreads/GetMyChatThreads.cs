namespace FourPrep.Modules.Chat.Presentation.ChatThreads;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Application.Paging;
using FourPrep.Common.Domain.Authorization;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Modules.Chat.Presentation.ChatThreads.Requests;
using FourPrep.Modules.Chat.Application.ChatThreads.GetChatThreads;

internal sealed class GetMyChatThreads : IEndpoint
{
    private const int DefaultRows = 20;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("staff/chat/threads", async (
            [FromServices] ISender sender,
            [AsParameters] GetChatThreadsRequest request) =>
        {
            var query = new GetMyChatThreadsQuery(
                new PagingRequest<ChatThreadSummaryResponse>(
                    request.First ?? 0,
                    request.Rows ?? DefaultRows,
                    request.SortField ?? "last_message_on_utc",
                    request.SortOrder ?? -1),
                request.ToFilter());

            var result = await sender.Send(query);

            return result;
        })
        .RequireAuthorization(Policies.Mentor)
        .Produces<PagedList<ChatThreadSummaryResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Chat / Staff");
    }
}
