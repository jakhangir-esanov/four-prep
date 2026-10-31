namespace FourPrep.Modules.Chat.Application.ChatMessages.GetChatMessages;

using Dapper;
using FourPrep.Common.Domain;
using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Chat.Domain.ChatThreads;
using FourPrep.Modules.Chat.Application.ChatThreads;
using FourPrep.Modules.Chat.Application.Abstractions.Students;
using FourPrep.Modules.Chat.Application.Abstractions.Authentication;

public sealed record GetChatMessagesQuery(
    Guid StudentUserId,
    ChatThreadKind Kind,
    ChatAudience Audience = ChatAudience.Family,
    Guid? BeforeMessageId = null,
    int Limit = GetChatMessagesQuery.DefaultLimit) : IQuery<IReadOnlyList<ChatMessageResponse>>
{
    public const int DefaultLimit = 50;
    private const int MaxLimit = 100;

    public int PageSize => Math.Clamp(Limit, 1, MaxLimit);
}

internal sealed class GetChatMessagesQueryHandler(
    ICurrentUser currentUser,
    IStudentService studentService,
    IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetChatMessagesQuery, IReadOnlyList<ChatMessageResponse>>
{
    private const string Sql =
        $"""
        SELECT
            m.id AS {nameof(ChatMessageResponse.Id)},
            m.sender_user_id AS {nameof(ChatMessageResponse.SenderUserId)},
            m.sender_name AS {nameof(ChatMessageResponse.SenderName)},
            m.sender AS {nameof(ChatMessageResponse.Sender)},
            m.sender_user_id = @UserId AS {nameof(ChatMessageResponse.FromMe)},
            m.body AS {nameof(ChatMessageResponse.Body)},
            m.sent_on_utc AS {nameof(ChatMessageResponse.SentOnUtc)}
        FROM chat.chat_messages m
        JOIN chat.chat_threads t ON t.id = m.thread_id
        WHERE t.student_user_id = @StudentUserId
          AND t.kind = @Kind
        """;

    private const string BeforeSql =
        """
          AND (m.sent_on_utc, m.id) < (
              SELECT b.sent_on_utc, b.id
              FROM chat.chat_messages b
              WHERE b.id = @BeforeMessageId AND b.thread_id = t.id)
        """;

    private const string OrderSql =
        """
        ORDER BY m.sent_on_utc DESC, m.id DESC
        LIMIT @PageSize
        """;

    public async Task<Result<IReadOnlyList<ChatMessageResponse>>> Handle(
        GetChatMessagesQuery request,
        CancellationToken cancellationToken)
    {
        var student = await ChatAccess.CheckAsync(
            studentService,
            currentUser,
            request.StudentUserId,
            request.Kind,
            request.Audience,
            cancellationToken);
        if (student.IsFailure)
        {
            return Result.Failure<IReadOnlyList<ChatMessageResponse>>(student.Error);
        }

        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        var beforeSql = request.BeforeMessageId is null ? string.Empty : BeforeSql;
        var sql = $"{Sql} {beforeSql} {OrderSql}";

        var messages = await connection.QueryAsync<ChatMessageResponse>(
            new CommandDefinition(
                sql,
                new
                {
                    currentUser.UserId,
                    request.StudentUserId,
                    Kind = (int)request.Kind,
                    request.BeforeMessageId,
                    request.PageSize
                },
                cancellationToken: cancellationToken));

        return request.Audience == ChatAudience.Family
            ? Result.Success<IReadOnlyList<ChatMessageResponse>>([.. messages.Select(message => message.ForFamily())])
            : Result.Success<IReadOnlyList<ChatMessageResponse>>(messages.AsList());
    }
}
