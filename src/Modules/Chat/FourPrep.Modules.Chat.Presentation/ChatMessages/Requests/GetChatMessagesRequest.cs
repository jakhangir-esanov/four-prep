namespace FourPrep.Modules.Chat.Presentation.ChatMessages.Requests;

internal sealed record GetChatMessagesRequest
{
    public Guid? Before { get; init; }
    public int? Limit { get; init; }
}
