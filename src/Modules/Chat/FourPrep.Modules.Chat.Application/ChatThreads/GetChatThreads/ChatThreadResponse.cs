namespace FourPrep.Modules.Chat.Application.ChatThreads.GetChatThreads;

using FourPrep.Modules.Chat.Domain.ChatThreads;
using FourPrep.Modules.Chat.Domain.ChatMessages;

public sealed record ChatThreadResponse
{
    public ChatThreadKind Kind { get; init; }
    public Guid? Id { get; init; }
    public bool CanWrite { get; init; }
    public string? LastMessagePreview { get; init; }
    public ChatMessageSender? LastMessageSender { get; init; }
    public DateTime? LastMessageOnUtc { get; init; }
    public int UnreadCount { get; init; }
}
