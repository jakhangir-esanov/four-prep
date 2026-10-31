namespace FourPrep.Modules.Chat.Domain.ChatThreads;

public interface IChatThreadRepository
{
    Task<ChatThread?> GetAsync(Guid studentUserId, ChatThreadKind kind, CancellationToken cancellationToken = default);

    void Insert(ChatThread thread);
}
