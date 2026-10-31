namespace FourPrep.Modules.Chat.UnitTests.ChatReadMarks;

using FourPrep.Modules.Chat.Domain.ChatThreads;
using FourPrep.Modules.Chat.Domain.ChatReadMarks;
using FourPrep.Modules.Chat.Application.ChatThreads;
using FourPrep.Modules.Chat.Application.ChatReadMarks.MarkChatThreadRead;

public class MarkChatThreadReadTests
{
    private static readonly Guid StudentUserId = Guid.NewGuid();
    private static readonly Guid MentorUserId = Guid.NewGuid();

    private readonly FakeStudentService _students = new();
    private readonly FakeChatThreadRepository _threads = new();
    private readonly FakeChatReadMarkRepository _readMarks = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeClock _clock = new();

    public MarkChatThreadReadTests()
    {
        _students.AddStudent(StudentUserId, MentorUserId);
    }

    [Fact]
    public async Task Handle_RecordsTheFirstRead()
    {
        var thread = _threads.Add(StudentUserId, ChatThreadKind.Mentor);

        var result = await Handler(StudentUserId).Handle(new MarkChatThreadReadCommand(StudentUserId, ChatThreadKind.Mentor), default);

        Assert.True(result.IsSuccess);
        var readMark = Assert.Single(_readMarks.ReadMarks);
        Assert.Equal(thread.Id, readMark.ThreadId);
        Assert.Equal(StudentUserId, readMark.UserId);
        Assert.Equal(_clock.UtcNow, readMark.LastReadOnUtc);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_MovesAnExistingReadMarkForward()
    {
        var thread = _threads.Add(StudentUserId, ChatThreadKind.Mentor);
        var readMark = ChatReadMark.Create(thread.Id, MentorUserId, _clock.UtcNow.AddHours(-1));
        _readMarks.Insert(readMark);

        await Handler(MentorUserId)
            .Handle(new MarkChatThreadReadCommand(StudentUserId, ChatThreadKind.Mentor, ChatAudience.Mentor), default);

        Assert.Same(readMark, Assert.Single(_readMarks.ReadMarks));
        Assert.Equal(_clock.UtcNow, readMark.LastReadOnUtc);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_DoesNothingWhenTheThreadHasNoMessagesYet()
    {
        var result = await Handler(StudentUserId).Handle(new MarkChatThreadReadCommand(StudentUserId, ChatThreadKind.Support), default);

        Assert.True(result.IsSuccess);
        Assert.Empty(_readMarks.ReadMarks);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_RefusesSomeoneOutsideTheFamily()
    {
        _threads.Add(StudentUserId, ChatThreadKind.Support);

        var result = await Handler(Guid.NewGuid()).Handle(new MarkChatThreadReadCommand(StudentUserId, ChatThreadKind.Support), default);

        Assert.Equal(ChatThreadErrors.NotYourStudent, result.Error);
        Assert.Empty(_readMarks.ReadMarks);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_RefusesAMentorWhoIsNotAssignedToTheStudent()
    {
        _threads.Add(StudentUserId, ChatThreadKind.Mentor);

        var result = await Handler(Guid.NewGuid())
            .Handle(new MarkChatThreadReadCommand(StudentUserId, ChatThreadKind.Mentor, ChatAudience.Mentor), default);

        Assert.Equal(ChatThreadErrors.NotYourStudent, result.Error);
        Assert.Empty(_readMarks.ReadMarks);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_LetsAnAdminMarkAnyThreadRead()
    {
        var adminUserId = Guid.NewGuid();
        _threads.Add(StudentUserId, ChatThreadKind.Support);

        var result = await Handler(adminUserId)
            .Handle(new MarkChatThreadReadCommand(StudentUserId, ChatThreadKind.Support, ChatAudience.Admin), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(adminUserId, Assert.Single(_readMarks.ReadMarks).UserId);
    }

    [Fact]
    public void MarkRead_NeverMovesTheReadMarkBack()
    {
        var readMark = ChatReadMark.Create(Guid.NewGuid(), StudentUserId, _clock.UtcNow);

        readMark.MarkRead(_clock.UtcNow.AddMinutes(-5));

        Assert.Equal(_clock.UtcNow, readMark.LastReadOnUtc);
    }

    private MarkChatThreadReadCommandHandler Handler(Guid userId) =>
        new(_unitOfWork, new FakeCurrentUser(userId), _students, _threads, _readMarks, _clock);
}
