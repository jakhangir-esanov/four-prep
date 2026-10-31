namespace FourPrep.Modules.Chat.Application.ChatThreads.GetChatThreads;

using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Paging;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Chat.Application.Abstractions.Authentication;

public sealed record GetChatThreadsQuery(
    PagingRequest<ChatThreadSummaryResponse> Paging,
    ChatThreadFilter? Filter = null) : IPagedListQuery<ChatThreadSummaryResponse>;

internal sealed class GetChatThreadsQueryHandler(
    ICurrentUser currentUser,
    IDbConnectionFactory dbConnectionFactory) : IPagedListQueryHandler<GetChatThreadsQuery, ChatThreadSummaryResponse>
{
    public async Task<PagedList<ChatThreadSummaryResponse>> Handle(
        GetChatThreadsQuery request,
        CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        var filter = (request.Filter ?? ChatThreadFilter.None) with { StudentUserIds = null };

        return await ChatThreadList.ReadPageAsync(
            connection,
            currentUser.UserId,
            request.Paging,
            filter,
            cancellationToken);
    }
}
