namespace FourPrep.Modules.Chat.Application.ChatThreads.GetChatThreads;

using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Paging;
using FourPrep.Common.Application.Messaging;
using FourPrep.Common.Application.Exceptions;
using FourPrep.Modules.Chat.Domain.ChatThreads;
using FourPrep.Modules.Chat.Application.Abstractions.Students;
using FourPrep.Modules.Chat.Application.Abstractions.Authentication;

public sealed record GetMyChatThreadsQuery(
    PagingRequest<ChatThreadSummaryResponse> Paging,
    ChatThreadFilter? Filter = null) : IPagedListQuery<ChatThreadSummaryResponse>;

internal sealed class GetMyChatThreadsQueryHandler(
    ICurrentUser currentUser,
    IStudentService studentService,
    IDbConnectionFactory dbConnectionFactory) : IPagedListQueryHandler<GetMyChatThreadsQuery, ChatThreadSummaryResponse>
{
    public async Task<PagedList<ChatThreadSummaryResponse>> Handle(
        GetMyChatThreadsQuery request,
        CancellationToken cancellationToken)
    {
        var mentees = await studentService.GetMenteeUserIdsAsync(currentUser.UserId, cancellationToken);
        if (mentees.IsFailure)
        {
            throw new ApplicationException(nameof(GetMyChatThreadsQuery), mentees.Error);
        }

        if (mentees.Data.Count == 0)
        {
            return new PagedList<ChatThreadSummaryResponse>([], 0);
        }

        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        var filter = (request.Filter ?? ChatThreadFilter.None) with
        {
            Kind = ChatThreadKind.Mentor,
            StudentUserIds = mentees.Data
        };

        return await ChatThreadList.ReadPageAsync(
            connection,
            currentUser.UserId,
            request.Paging,
            filter,
            cancellationToken);
    }
}
