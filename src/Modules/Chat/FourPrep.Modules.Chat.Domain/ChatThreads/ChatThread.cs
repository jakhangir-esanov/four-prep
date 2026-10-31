namespace FourPrep.Modules.Chat.Domain.ChatThreads;

public sealed class ChatThread
{
    public Guid Id { get; set; }
    public Guid StudentUserId { get; set; }
    public ChatThreadKind Kind { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset UpdatedOnUtc { get; set; }

    public static ChatThread Create(Guid studentUserId, ChatThreadKind kind, string studentName, DateTime utcNow)
    {
        return new ChatThread
        {
            Id = Guid.NewGuid(),
            StudentUserId = studentUserId,
            Kind = kind,
            StudentName = studentName,
            CreatedOnUtc = utcNow,
            UpdatedOnUtc = utcNow
        };
    }

    public void RenameStudent(string studentName, DateTime utcNow)
    {
        if (StudentName == studentName)
        {
            return;
        }

        StudentName = studentName;
        UpdatedOnUtc = utcNow;
    }
}
