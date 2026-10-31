namespace FourPrep.Modules.Chat.Application.ChatMessages.GetChatMessages;

using FourPrep.Modules.Chat.Domain.ChatMessages;

public sealed record ChatMessageResponse
{
    public Guid Id { get; init; }
    public Guid? SenderUserId { get; init; }
    public string? SenderName { get; init; }
    public ChatMessageSender Sender { get; init; }
    public bool FromMe { get; init; }
    public string Body { get; init; } = string.Empty;
    public DateTime SentOnUtc { get; init; }

    public ChatMessageResponse ForFamily() =>
        Sender == ChatMessageSender.Staff ? this with { SenderUserId = null, SenderName = null } : this;
}
