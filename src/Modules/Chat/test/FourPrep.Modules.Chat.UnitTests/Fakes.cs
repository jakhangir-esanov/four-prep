namespace FourPrep.Modules.Chat.UnitTests;

using System.Data.Common;
using FourPrep.Common.Domain;
using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Clock;
using FourPrep.Modules.Chat.Domain.ChatThreads;
using FourPrep.Modules.Chat.Domain.ChatMessages;
using FourPrep.Modules.Chat.Domain.ChatReadMarks;
using FourPrep.Modules.Chat.Application.Abstractions.Data;
using FourPrep.Modules.Chat.Application.Abstractions.Students;
using FourPrep.Modules.Chat.Application.Abstractions.Authentication;

internal sealed class FakeClock : IDateTimeProvider
{
    public DateTime UtcNow { get; set; } = new(2026, 10, 31, 9, 0, 0, DateTimeKind.Utc);
}

internal sealed class FakeCurrentUser(Guid userId, string userName = "current.user") : ICurrentUser
{
    public Guid UserId { get; } = userId;

    public string UserName { get; } = userName;
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveCount++;
        return Task.FromResult(0);
    }

    public Task<DbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();
}

internal sealed class FakeChatThreadRepository : IChatThreadRepository
{
    public List<ChatThread> Threads { get; } = [];

    public ChatThread Add(Guid studentUserId, ChatThreadKind kind, string studentName = "Aziza Karimova")
    {
        var thread = ChatThread.Create(
            studentUserId,
            kind,
            studentName,
            new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc));
        Threads.Add(thread);
        return thread;
    }

    public Task<ChatThread?> GetAsync(Guid studentUserId, ChatThreadKind kind, CancellationToken cancellationToken = default) =>
        Task.FromResult(Threads.SingleOrDefault(x => x.StudentUserId == studentUserId && x.Kind == kind));

    public void Insert(ChatThread thread) => Threads.Add(thread);
}

internal sealed class FakeChatMessageRepository : IChatMessageRepository
{
    public List<ChatMessage> Messages { get; } = [];

    public void Insert(ChatMessage message) => Messages.Add(message);
}

internal sealed class FakeChatReadMarkRepository : IChatReadMarkRepository
{
    public List<ChatReadMark> ReadMarks { get; } = [];

    public Task<ChatReadMark?> GetAsync(Guid threadId, Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(ReadMarks.SingleOrDefault(x => x.ThreadId == threadId && x.UserId == userId));

    public void Insert(ChatReadMark readMark) => ReadMarks.Add(readMark);
}

internal sealed class FakeStudentService : IStudentService
{
    public static readonly Error NotFound = Error.NotFound("Student.NotFound", "Not found.", "Topilmadi.", "Не найден.");

    public HashSet<(Guid UserId, Guid StudentUserId)> Access { get; } = [];

    public Dictionary<Guid, ChatStudent> Students { get; } = [];

    public Error? AccessError { get; init; }

    public void AddStudent(Guid studentUserId, Guid? mentorUserId = null, Guid? parentUserId = null)
    {
        Access.Add((studentUserId, studentUserId));
        if (parentUserId is { } parent)
        {
            Access.Add((parent, studentUserId));
        }

        Students[studentUserId] = new ChatStudent(studentUserId, "Aziza Karimova", mentorUserId);
    }

    public Task<Result<bool>> HasFamilyAccessAsync(Guid userId, Guid studentUserId, CancellationToken cancellationToken = default) =>
        Task.FromResult(AccessError is { } error
            ? Result.Failure<bool>(error)
            : Result.Success(Access.Contains((userId, studentUserId))));

    public Task<Result<ChatStudent>> GetStudentAsync(Guid studentUserId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Students.TryGetValue(studentUserId, out var student)
            ? Result.Success(student)
            : Result.Failure<ChatStudent>(NotFound));

    public Task<Result<IReadOnlyList<Guid>>> GetMenteeUserIdsAsync(
        Guid mentorUserId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success<IReadOnlyList<Guid>>(
            [.. Students.Values.Where(student => student.MentorUserId == mentorUserId).Select(student => student.UserId)]));
}

internal sealed class UnusedConnectionFactory : IDbConnectionFactory
{
    public ValueTask<DbConnection> OpenConnectionAsync() =>
        throw new InvalidOperationException("The handler should not reach the database.");
}
