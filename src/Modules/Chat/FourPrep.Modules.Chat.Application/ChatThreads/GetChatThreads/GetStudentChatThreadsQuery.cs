namespace FourPrep.Modules.Chat.Application.ChatThreads.GetChatThreads;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Chat.Domain.ChatThreads;
using FourPrep.Modules.Chat.Application.Abstractions.Students;
using FourPrep.Modules.Chat.Application.Abstractions.Authentication;

public sealed record GetStudentChatThreadsQuery(Guid StudentUserId) : IQuery<IReadOnlyList<ChatThreadResponse>>;

internal sealed class GetStudentChatThreadsQueryHandler(
    ICurrentUser currentUser,
    IStudentService studentService,
    IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetStudentChatThreadsQuery, IReadOnlyList<ChatThreadResponse>>
{
    private static readonly ChatThreadKind[] Kinds = [ChatThreadKind.Mentor, ChatThreadKind.Support];

    public async Task<Result<IReadOnlyList<ChatThreadResponse>>> Handle(
        GetStudentChatThreadsQuery request,
        CancellationToken cancellationToken)
    {
        var student = await ChatAccess.CheckAsync(
            studentService,
            currentUser,
            request.StudentUserId,
            ChatThreadKind.Support,
            ChatAudience.Family,
            cancellationToken);
        if (student.IsFailure)
        {
            return Result.Failure<IReadOnlyList<ChatThreadResponse>>(student.Error);
        }

        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        var threads = await ChatThreadList.ReadForStudentAsync(
            connection,
            currentUser.UserId,
            request.StudentUserId,
            cancellationToken);

        return Result.Success(ToResponses(threads, student.Data.MentorUserId is not null));
    }

    internal static IReadOnlyList<ChatThreadResponse> ToResponses(
        IEnumerable<ChatThreadSummaryResponse> threads,
        bool hasMentor)
    {
        var byKind = threads.ToDictionary(thread => thread.Kind);

        return
        [
            .. Kinds.Select(kind => ToResponse(
                kind,
                byKind.GetValueOrDefault(kind),
                kind != ChatThreadKind.Mentor || hasMentor))
        ];
    }

    private static ChatThreadResponse ToResponse(ChatThreadKind kind, ChatThreadSummaryResponse? thread, bool canWrite) =>
        new()
        {
            Kind = kind,
            Id = thread?.Id,
            CanWrite = canWrite,
            LastMessagePreview = thread?.LastMessagePreview,
            LastMessageSender = thread?.LastMessageSender,
            LastMessageOnUtc = thread?.LastMessageOnUtc,
            UnreadCount = thread?.UnreadCount ?? 0
        };
}
