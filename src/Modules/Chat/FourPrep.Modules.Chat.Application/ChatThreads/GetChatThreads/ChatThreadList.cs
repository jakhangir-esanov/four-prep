namespace FourPrep.Modules.Chat.Application.ChatThreads.GetChatThreads;

using Dapper;
using System.Data.Common;
using FourPrep.Common.Application.Paging;
using FourPrep.Modules.Chat.Domain.ChatMessages;

internal static class ChatThreadList
{
    private const int PreviewLength = 200;
    private const int FamilySender = (int)ChatMessageSender.Family;

    private const string InboxSql =
        """
        WITH inbox AS (
            SELECT
                t.id,
                t.student_user_id,
                t.student_name,
                t.kind,
                last_message.preview AS last_message_preview,
                last_message.sender AS last_message_sender,
                last_message.sent_on_utc AS last_message_on_utc,
                COALESCE(last_message.sender = @FamilySender, FALSE) AS awaiting_reply,
                (SELECT COUNT(*)::int
                 FROM chat.chat_messages m
                 WHERE m.thread_id = t.id
                   AND m.sender_user_id <> @UserId
                   AND m.sent_on_utc > COALESCE(r.last_read_on_utc, '-infinity'::timestamptz)) AS unread_count
            FROM chat.chat_threads t
            LEFT JOIN chat.chat_read_marks r ON r.thread_id = t.id AND r.user_id = @UserId
            LEFT JOIN LATERAL (
                SELECT LEFT(m.body, @PreviewLength) AS preview, m.sender, m.sent_on_utc
                FROM chat.chat_messages m
                WHERE m.thread_id = t.id
                ORDER BY m.sent_on_utc DESC, m.id DESC
                LIMIT 1) last_message ON TRUE)
        """;

    private const string SelectSql =
        $"""
        SELECT
            i.id AS {nameof(ChatThreadSummaryResponse.Id)},
            i.student_user_id AS {nameof(ChatThreadSummaryResponse.StudentUserId)},
            i.student_name AS {nameof(ChatThreadSummaryResponse.StudentName)},
            i.kind AS {nameof(ChatThreadSummaryResponse.Kind)},
            i.last_message_preview AS {nameof(ChatThreadSummaryResponse.LastMessagePreview)},
            i.last_message_sender AS {nameof(ChatThreadSummaryResponse.LastMessageSender)},
            i.last_message_on_utc AS {nameof(ChatThreadSummaryResponse.LastMessageOnUtc)},
            i.awaiting_reply AS {nameof(ChatThreadSummaryResponse.AwaitingReply)},
            i.unread_count AS {nameof(ChatThreadSummaryResponse.UnreadCount)}
        FROM inbox i
        """;

    public static async Task<IReadOnlyList<ChatThreadSummaryResponse>> ReadForStudentAsync(
        DbConnection connection,
        Guid userId,
        Guid studentUserId,
        CancellationToken cancellationToken)
    {
        const string sql = $"{InboxSql}\n{SelectSql}\nWHERE i.student_user_id = @StudentUserId ORDER BY i.kind";

        var threads = await connection.QueryAsync<ChatThreadSummaryResponse>(
            new CommandDefinition(
                sql,
                new { UserId = userId, StudentUserId = studentUserId, FamilySender, PreviewLength },
                cancellationToken: cancellationToken));

        return threads.AsList();
    }

    public static async Task<PagedList<ChatThreadSummaryResponse>> ReadPageAsync(
        DbConnection connection,
        Guid userId,
        PagingRequest<ChatThreadSummaryResponse> paging,
        ChatThreadFilter filter,
        CancellationToken cancellationToken)
    {
        var conditions = Conditions(filter);
        string whereSql = conditions.Count == 0 ? string.Empty : $"WHERE {string.Join(" AND ", conditions)}";

        var dataSql =
            $"""
            {InboxSql}
            {SelectSql}
            {whereSql}
            ORDER BY i.{paging.SortBy} {paging.SortDirection} NULLS LAST, i.id
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        var countSql = $"{InboxSql}\nSELECT COUNT(*) FROM inbox i {whereSql}";

        var param = new
        {
            UserId = userId,
            FamilySender,
            PreviewLength,
            Kind = (int?)filter.Kind,
            filter.AwaitingReply,
            StudentUserIds = filter.StudentUserIds?.ToArray(),
            Search = filter.SearchPattern,
            Offset = paging.First,
            PageSize = paging.Rows
        };

        var threads = await connection.QueryAsync<ChatThreadSummaryResponse>(
            new CommandDefinition(dataSql, param, cancellationToken: cancellationToken));

        var totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, param, cancellationToken: cancellationToken));

        return new PagedList<ChatThreadSummaryResponse>(threads.AsList(), totalCount);
    }

    private static List<string> Conditions(ChatThreadFilter filter)
    {
        var conditions = new List<string>();

        if (filter.Kind is not null)
        {
            conditions.Add("i.kind = @Kind");
        }

        if (filter.AwaitingReply is not null)
        {
            conditions.Add("i.awaiting_reply = @AwaitingReply");
        }

        if (filter.UnreadOnly is true)
        {
            conditions.Add("i.unread_count > 0");
        }

        if (filter.StudentUserIds is not null)
        {
            conditions.Add("i.student_user_id = ANY(@StudentUserIds)");
        }

        if (filter.SearchPattern is not null)
        {
            conditions.Add("i.student_name ILIKE @Search");
        }

        return conditions;
    }
}
