namespace FourPrep.Modules.Chat.UnitTests.ChatMessages;

using FourPrep.Modules.Chat.Domain.ChatThreads;
using FourPrep.Modules.Chat.Domain.ChatMessages;
using FourPrep.Modules.Chat.Application.ChatThreads;
using FourPrep.Modules.Chat.Application.ChatMessages.GetChatMessages;

public class GetChatMessagesTests
{
    private static readonly Guid StudentUserId = Guid.NewGuid();
    private static readonly Guid MentorUserId = Guid.NewGuid();

    private static readonly GetChatMessagesQueryValidator Validator = new();

    private readonly FakeStudentService _students = new();

    public GetChatMessagesTests()
    {
        _students.AddStudent(StudentUserId, MentorUserId);
    }

    [Fact]
    public async Task Handle_RefusesSomeoneOutsideTheFamilyBeforeReadingMessages()
    {
        var result = await Handler(Guid.NewGuid()).Handle(new GetChatMessagesQuery(StudentUserId, ChatThreadKind.Mentor), default);

        Assert.Equal(ChatThreadErrors.NotYourStudent, result.Error);
    }

    [Fact]
    public async Task Handle_RefusesAMentorWhoIsNoLongerAssigned()
    {
        var result = await Handler(Guid.NewGuid())
            .Handle(new GetChatMessagesQuery(StudentUserId, ChatThreadKind.Mentor, ChatAudience.Mentor), default);

        Assert.Equal(ChatThreadErrors.NotYourStudent, result.Error);
    }

    [Fact]
    public async Task Handle_KeepsTheMentorOutOfTheSupportThread()
    {
        var result = await Handler(MentorUserId)
            .Handle(new GetChatMessagesQuery(StudentUserId, ChatThreadKind.Support, ChatAudience.Mentor), default);

        Assert.Equal(ChatThreadErrors.NotYourStudent, result.Error);
    }

    [Fact]
    public async Task Handle_DoesNotTellAMentorWhetherAnUnknownStudentExists()
    {
        var result = await Handler(MentorUserId)
            .Handle(new GetChatMessagesQuery(Guid.NewGuid(), ChatThreadKind.Mentor, ChatAudience.Mentor), default);

        Assert.Equal(ChatThreadErrors.NotYourStudent, result.Error);
    }

    [Fact]
    public void ForFamily_HidesWhichStaffMemberWrote()
    {
        var message = new ChatMessageResponse
        {
            SenderUserId = MentorUserId,
            SenderName = "mentor.aziz",
            Sender = ChatMessageSender.Staff,
            Body = "Aim for 1400 or more."
        };

        var forFamily = message.ForFamily();

        Assert.Null(forFamily.SenderUserId);
        Assert.Null(forFamily.SenderName);
        Assert.Equal(ChatMessageSender.Staff, forFamily.Sender);
        Assert.Equal(message.Body, forFamily.Body);
    }

    [Fact]
    public void ForFamily_KeepsTheFamilySender()
    {
        var message = new ChatMessageResponse
        {
            SenderUserId = StudentUserId,
            SenderName = "Aziza Karimova",
            Sender = ChatMessageSender.Family
        };

        Assert.Equal(message, message.ForFamily());
    }

    [Fact]
    public void Validate_AcceptsTheDefaultPage()
    {
        Assert.True(Validator.Validate(new GetChatMessagesQuery(StudentUserId, ChatThreadKind.Support)).IsValid);
    }

    [Fact]
    public void Validate_WhenTheThreadKindIsUnknown_Fails()
    {
        var result = Validator.Validate(new GetChatMessagesQuery(StudentUserId, (ChatThreadKind)9));

        Assert.Contains(result.Errors, error => error.PropertyName == "Kind" && error.ErrorCode == "EnumValidator");
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(50, 50)]
    [InlineData(500, 100)]
    public void PageSize_KeepsTheLimitBetweenOneAndAHundred(int limit, int pageSize)
    {
        Assert.Equal(pageSize, new GetChatMessagesQuery(StudentUserId, ChatThreadKind.Support, Limit: limit).PageSize);
    }

    private GetChatMessagesQueryHandler Handler(Guid userId) =>
        new(new FakeCurrentUser(userId), _students, new UnusedConnectionFactory());
}
