namespace FourPrep.Modules.Chat.Application.ChatThreads.GetChatThreads;

using FourPrep.Common.Application.Data;
using FourPrep.Modules.Chat.Domain.ChatThreads;

public sealed record ChatThreadFilter(
    ChatThreadKind? Kind = null,
    bool? AwaitingReply = null,
    bool? UnreadOnly = null,
    string? Search = null,
    IReadOnlyList<Guid>? StudentUserIds = null)
{
    public static readonly ChatThreadFilter None = new();

    public string? SearchPattern => LikePatterns.Contains(Search);
}
