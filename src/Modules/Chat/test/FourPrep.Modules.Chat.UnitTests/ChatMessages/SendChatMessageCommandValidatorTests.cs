namespace FourPrep.Modules.Chat.UnitTests.ChatMessages;

using FourPrep.Modules.Chat.Domain.ChatThreads;
using FourPrep.Modules.Chat.Domain.ChatMessages;
using FourPrep.Modules.Chat.Application.ChatThreads;
using FourPrep.Modules.Chat.Application.ChatMessages.SendChatMessage;

public class SendChatMessageCommandValidatorTests
{
    private static readonly SendChatMessageCommandValidator Validator = new();

    [Fact]
    public void Validate_AcceptsAMessage()
    {
        Assert.True(Validator.Validate(SendChatMessageTests.Command(ChatThreadKind.Mentor)).IsValid);
    }

    [Fact]
    public void Validate_AcceptsAMessageOfTheMaximumLengthWithSurroundingSpaces()
    {
        var command = SendChatMessageTests.Command(ChatThreadKind.Support) with { Body = $"  {new string('a', 4000)}  " };

        Assert.True(Validator.Validate(command).IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenTheBodyIsBlank_Fails(string body)
    {
        AssertFails(SendChatMessageTests.Command(ChatThreadKind.Support) with { Body = body }, "Body", ChatMessageErrors.BodyRequired.Code);
    }

    [Fact]
    public void Validate_WhenTheBodyIsTooLong_Fails()
    {
        AssertFails(
            SendChatMessageTests.Command(ChatThreadKind.Support) with { Body = new string('a', 4001) },
            "Body",
            ChatMessageErrors.BodyRequired.Code);
    }

    [Fact]
    public void Validate_WhenTheStudentIsMissing_Fails()
    {
        AssertFails(
            SendChatMessageTests.Command(ChatThreadKind.Support) with { StudentUserId = Guid.Empty },
            "StudentUserId",
            "NotEmptyValidator");
    }

    [Fact]
    public void Validate_WhenTheThreadKindIsUnknown_Fails()
    {
        AssertFails(SendChatMessageTests.Command((ChatThreadKind)9), "Kind", "EnumValidator");
    }

    [Fact]
    public void Validate_WhenTheAudienceIsUnknown_Fails()
    {
        AssertFails(
            SendChatMessageTests.Command(ChatThreadKind.Support) with { Audience = (ChatAudience)9 },
            "Audience",
            "EnumValidator");
    }

    private static void AssertFails(SendChatMessageCommand command, string propertyName, string errorCode)
    {
        var result = Validator.Validate(command);

        Assert.Contains(result.Errors, error => error.PropertyName == propertyName && error.ErrorCode == errorCode);
    }
}
