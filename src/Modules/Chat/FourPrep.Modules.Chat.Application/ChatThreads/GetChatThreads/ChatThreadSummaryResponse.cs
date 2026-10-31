namespace FourPrep.Modules.Chat.Application.ChatThreads.GetChatThreads;

using FourPrep.Modules.Chat.Domain.ChatThreads;
using FourPrep.Modules.Chat.Domain.ChatMessages;

public sealed record ChatThreadSummaryResponse
{
    public Guid Id { get; init; }
    public Guid StudentUserId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public ChatThreadKind Kind { get; init; }
    public string? LastMessagePreview { get; init; }
    public ChatMessageSender? LastMessageSender { get; init; }
    public DateTime? LastMessageOnUtc { get; init; }
    public bool AwaitingReply { get; init; }
    public int UnreadCount { get; init; }
}
