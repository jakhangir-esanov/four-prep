namespace FourPrep.Modules.Chat.UnitTests.ChatThreads;

using FourPrep.Common.Application.Paging;
using FourPrep.Modules.Chat.Domain.ChatThreads;
using FourPrep.Modules.Chat.Domain.ChatMessages;
using FourPrep.Modules.Chat.Application.ChatThreads.GetChatThreads;

public class GetChatThreadsTests
{
    private static readonly Guid StudentUserId = Guid.NewGuid();

    private readonly FakeStudentService _students = new();

    public GetChatThreadsTests()
    {
        _students.AddStudent(StudentUserId, Guid.NewGuid());
    }

    [Fact]
    public async Task GetStudentChatThreads_RefusesSomeoneOutsideTheFamily()
    {
        var handler = new GetStudentChatThreadsQueryHandler(
            new FakeCurrentUser(Guid.NewGuid()),
            _students,
            new UnusedConnectionFactory());

        var result = await handler.Handle(new GetStudentChatThreadsQuery(StudentUserId), default);

        Assert.Equal(ChatThreadErrors.NotYourStudent, result.Error);
    }

    [Fact]
    public void ToResponses_ListsBothThreadsEvenBeforeTheFirstMessage()
    {
        var responses = GetStudentChatThreadsQueryHandler.ToResponses([], hasMentor: true);

        Assert.Equal([ChatThreadKind.Mentor, ChatThreadKind.Support], responses.Select(thread => thread.Kind));
        Assert.All(responses, thread =>
        {
            Assert.Null(thread.Id);
            Assert.True(thread.CanWrite);
            Assert.Equal(0, thread.UnreadCount);
        });
    }

    [Fact]
    public void ToResponses_ClosesTheMentorThreadWhileNoMentorIsAssigned()
    {
        var responses = GetStudentChatThreadsQueryHandler.ToResponses([], hasMentor: false);

        Assert.False(responses.Single(thread => thread.Kind == ChatThreadKind.Mentor).CanWrite);
        Assert.True(responses.Single(thread => thread.Kind == ChatThreadKind.Support).CanWrite);
    }

    [Fact]
    public void ToResponses_CarriesTheLastMessageAndUnreadCount()
    {
        var support = new ChatThreadSummaryResponse
        {
            Id = Guid.NewGuid(),
            StudentUserId = StudentUserId,
            Kind = ChatThreadKind.Support,
            LastMessagePreview = "A mentor will be assigned today.",
            LastMessageSender = ChatMessageSender.Staff,
            LastMessageOnUtc = new DateTime(2026, 10, 31, 8, 0, 0, DateTimeKind.Utc),
            UnreadCount = 2
        };

        var response = GetStudentChatThreadsQueryHandler.ToResponses([support], hasMentor: true)
            .Single(thread => thread.Kind == ChatThreadKind.Support);

        Assert.Equal(support.Id, response.Id);
        Assert.Equal(support.LastMessagePreview, response.LastMessagePreview);
        Assert.Equal(ChatMessageSender.Staff, response.LastMessageSender);
        Assert.Equal(support.LastMessageOnUtc, response.LastMessageOnUtc);
        Assert.Equal(2, response.UnreadCount);
    }

    [Fact]
    public void Validate_WhenTheStudentIsMissing_Fails()
    {
        var error = Assert.Single(new GetStudentChatThreadsQueryValidator().Validate(new GetStudentChatThreadsQuery(Guid.Empty)).Errors);

        Assert.Equal(nameof(GetStudentChatThreadsQuery.StudentUserId), error.PropertyName);
        Assert.Equal("NotEmptyValidator", error.ErrorCode);
    }

    [Fact]
    public async Task GetMyChatThreads_ReturnsAnEmptyInboxToAMentorWithoutMentees()
    {
        var handler = new GetMyChatThreadsQueryHandler(
            new FakeCurrentUser(Guid.NewGuid()),
            _students,
            new UnusedConnectionFactory());

        var result = await handler.Handle(
            new GetMyChatThreadsQuery(new PagingRequest<ChatThreadSummaryResponse>(0, 20, "last_message_on_utc", -1)),
            default);

        Assert.Empty(result.Data);
        Assert.Equal(0, result.TotalCount);
    }
}
