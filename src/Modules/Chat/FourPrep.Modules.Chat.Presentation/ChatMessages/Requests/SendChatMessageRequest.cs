namespace FourPrep.Modules.Chat.Presentation.ChatMessages.Requests;

internal sealed record SendChatMessageRequest
{
    public string Body { get; init; } = string.Empty;
}
