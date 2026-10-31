namespace FourPrep.Modules.Chat.Presentation.ChatThreads.Requests;

using FourPrep.Modules.Chat.Domain.ChatThreads;
using FourPrep.Modules.Chat.Application.ChatThreads.GetChatThreads;

internal sealed record GetChatThreadsRequest
{
    public int? First { get; init; }
    public int? Rows { get; init; }
    public string? SortField { get; init; }
    public int? SortOrder { get; init; }

    public ChatThreadKind? Kind { get; init; }
    public bool? AwaitingReply { get; init; }
    public bool? UnreadOnly { get; init; }
    public string? Search { get; init; }

    public ChatThreadFilter ToFilter() => new(Kind, AwaitingReply, UnreadOnly, Search);
}
