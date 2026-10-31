namespace FourPrep.Modules.Chat.UnitTests.ChatMessages;

using FourPrep.Common.Domain;
using FourPrep.Modules.Chat.Domain.ChatThreads;
using FourPrep.Modules.Chat.Domain.ChatMessages;
using FourPrep.Modules.Chat.Application.ChatThreads;
using FourPrep.Modules.Chat.Application.ChatMessages.SendChatMessage;

public class SendChatMessageTests
{
    private static readonly Guid StudentUserId = Guid.NewGuid();
    private static readonly Guid ParentUserId = Guid.NewGuid();
    private static readonly Guid MentorUserId = Guid.NewGuid();

    private readonly FakeStudentService _students = new();
    private readonly FakeChatThreadRepository _threads = new();
    private readonly FakeChatMessageRepository _messages = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    public SendChatMessageTests()
    {
        _students.AddStudent(StudentUserId, MentorUserId, ParentUserId);
    }

    [Fact]
    public async Task Handle_OpensTheThreadOnTheFirstMessage()
    {
        var result = await Handler(StudentUserId).Handle(Command(ChatThreadKind.Support), default);

        Assert.True(result.IsSuccess);
        var thread = Assert.Single(_threads.Threads);
        Assert.Equal(StudentUserId, thread.StudentUserId);
        Assert.Equal(ChatThreadKind.Support, thread.Kind);
        Assert.Equal("Aziza Karimova", thread.StudentName);
        var message = Assert.Single(_messages.Messages);
        Assert.Equal(result.Data, message.Id);
        Assert.Equal(thread.Id, message.ThreadId);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_SignsTheStudentsMessageWithTheProfileName()
    {
        await Handler(StudentUserId).Handle(Command(ChatThreadKind.Support) with { Body = "  When is my consultation?  " }, default);

        var message = Assert.Single(_messages.Messages);
        Assert.Equal(StudentUserId, message.SenderUserId);
        Assert.Equal("Aziza Karimova", message.SenderName);
        Assert.Equal(ChatMessageSender.Family, message.Sender);
        Assert.Equal("When is my consultation?", message.Body);
        Assert.Equal(new FakeClock().UtcNow, message.SentOnUtc);
    }

    [Fact]
    public async Task Handle_LetsALinkedParentWriteUnderTheirUserName()
    {
        var result = await Handler(ParentUserId, "parent.karimova").Handle(Command(ChatThreadKind.Mentor), default);

        Assert.True(result.IsSuccess);
        var message = Assert.Single(_messages.Messages);
        Assert.Equal(ParentUserId, message.SenderUserId);
        Assert.Equal("parent.karimova", message.SenderName);
        Assert.Equal(ChatMessageSender.Family, message.Sender);
    }

    [Fact]
    public async Task Handle_AddsToTheExistingThreadAndRefreshesTheStudentName()
    {
        var existing = _threads.Add(StudentUserId, ChatThreadKind.Support, "Aziza K.");

        await Handler(StudentUserId).Handle(Command(ChatThreadKind.Support), default);

        Assert.Same(existing, Assert.Single(_threads.Threads));
        Assert.Equal("Aziza Karimova", existing.StudentName);
        Assert.Equal(existing.Id, Assert.Single(_messages.Messages).ThreadId);
    }

    [Fact]
    public async Task Handle_RefusesSomeoneOutsideTheFamily()
    {
        var result = await Handler(Guid.NewGuid()).Handle(Command(ChatThreadKind.Support), default);

        AssertRefused(result, ChatThreadErrors.NotYourStudent);
    }

    [Fact]
    public async Task Handle_PassesThroughAFamilyAccessFailure()
    {
        var students = new FakeStudentService { AccessError = FakeStudentService.NotFound };
        students.AddStudent(StudentUserId, MentorUserId);

        var result = await Handler(StudentUserId, students: students).Handle(Command(ChatThreadKind.Support), default);

        AssertRefused(result, FakeStudentService.NotFound);
    }

    [Fact]
    public async Task Handle_RefusesTheMentorThreadBeforeAMentorIsAssigned()
    {
        var studentUserId = Guid.NewGuid();
        _students.AddStudent(studentUserId);

        var result = await Handler(studentUserId).Handle(Command(ChatThreadKind.Mentor, studentUserId), default);

        AssertRefused(result, ChatThreadErrors.NoMentor);
    }

    [Fact]
    public async Task Handle_LetsTheAssignedMentorReply()
    {
        var result = await Handler(MentorUserId, "mentor.aziz")
            .Handle(Command(ChatThreadKind.Mentor) with { Audience = ChatAudience.Mentor }, default);

        Assert.True(result.IsSuccess);
        var message = Assert.Single(_messages.Messages);
        Assert.Equal("mentor.aziz", message.SenderName);
        Assert.Equal(ChatMessageSender.Staff, message.Sender);
    }

    [Fact]
    public async Task Handle_RefusesAMentorWhoIsNotAssignedToTheStudent()
    {
        var result = await Handler(Guid.NewGuid())
            .Handle(Command(ChatThreadKind.Mentor) with { Audience = ChatAudience.Mentor }, default);

        AssertRefused(result, ChatThreadErrors.NotYourStudent);
    }

    [Fact]
    public async Task Handle_KeepsTheMentorOutOfTheSupportThread()
    {
        var result = await Handler(MentorUserId)
            .Handle(Command(ChatThreadKind.Support) with { Audience = ChatAudience.Mentor }, default);

        AssertRefused(result, ChatThreadErrors.NotYourStudent);
    }

    [Fact]
    public async Task Handle_DoesNotTellAMentorWhetherAnUnknownStudentExists()
    {
        var result = await Handler(MentorUserId)
            .Handle(Command(ChatThreadKind.Mentor, Guid.NewGuid()) with { Audience = ChatAudience.Mentor }, default);

        AssertRefused(result, ChatThreadErrors.NotYourStudent);
    }

    [Fact]
    public async Task Handle_LetsAnAdminAnswerSupport()
    {
        var result = await Handler(Guid.NewGuid(), "mentor.admin")
            .Handle(Command(ChatThreadKind.Support) with { Audience = ChatAudience.Admin }, default);

        Assert.True(result.IsSuccess);
        Assert.Equal(ChatMessageSender.Staff, Assert.Single(_messages.Messages).Sender);
    }

    [Fact]
    public async Task Handle_FailsWhenTheStudentProfileIsMissing()
    {
        var result = await Handler(Guid.NewGuid())
            .Handle(Command(ChatThreadKind.Support, Guid.NewGuid()) with { Audience = ChatAudience.Admin }, default);

        AssertRefused(result, FakeStudentService.NotFound);
    }

    internal static SendChatMessageCommand Command(ChatThreadKind kind, Guid? studentUserId = null) =>
        new(studentUserId ?? StudentUserId, kind, "Which universities fit my budget?");

    private void AssertRefused(Result<Guid> result, Error error)
    {
        Assert.Equal(error, result.Error);
        Assert.Empty(_threads.Threads);
        Assert.Empty(_messages.Messages);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    private SendChatMessageCommandHandler Handler(
        Guid userId,
        string userName = "current.user",
        FakeStudentService? students = null) =>
        new(
            _unitOfWork,
            new FakeCurrentUser(userId, userName),
            students ?? _students,
            _threads,
            _messages,
            new FakeClock());
}
