namespace FourPrep.Modules.Chat.Domain.ChatMessages;

public interface IChatMessageRepository
{
    void Insert(ChatMessage message);
}
