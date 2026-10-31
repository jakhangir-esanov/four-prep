namespace FourPrep.Modules.Chat.Domain.ChatReadMarks;

public interface IChatReadMarkRepository
{
    Task<ChatReadMark?> GetAsync(Guid threadId, Guid userId, CancellationToken cancellationToken = default);

    void Insert(ChatReadMark readMark);
}
