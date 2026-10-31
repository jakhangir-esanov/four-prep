namespace FourPrep.Modules.Chat.Domain.ChatReadMarks;

public sealed class ChatReadMark
{
    public Guid Id { get; set; }
    public Guid ThreadId { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset LastReadOnUtc { get; set; }

    public static ChatReadMark Create(Guid threadId, Guid userId, DateTime utcNow)
    {
        return new ChatReadMark
        {
            Id = Guid.NewGuid(),
            ThreadId = threadId,
            UserId = userId,
            LastReadOnUtc = utcNow
        };
    }

    public void MarkRead(DateTime utcNow)
    {
        if (utcNow > LastReadOnUtc)
        {
            LastReadOnUtc = utcNow;
        }
    }
}
