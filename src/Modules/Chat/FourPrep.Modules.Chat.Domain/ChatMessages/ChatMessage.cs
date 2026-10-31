namespace FourPrep.Modules.Chat.Domain.ChatMessages;

public sealed class ChatMessage
{
    public Guid Id { get; set; }
    public Guid ThreadId { get; set; }
    public Guid SenderUserId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public ChatMessageSender Sender { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTimeOffset SentOnUtc { get; set; }

    public static ChatMessage Create(
        Guid threadId,
        Guid senderUserId,
        string senderName,
        ChatMessageSender sender,
        string body,
        DateTime utcNow)
    {
        return new ChatMessage
        {
            Id = Guid.NewGuid(),
            ThreadId = threadId,
            SenderUserId = senderUserId,
            SenderName = senderName,
            Sender = sender,
            Body = body.Trim(),
            SentOnUtc = utcNow
        };
    }
}
