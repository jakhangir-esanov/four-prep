namespace FourPrep.Modules.Chat.Infrastructure.ChatMessages;

using FourPrep.Modules.Chat.Domain.ChatMessages;
using FourPrep.Modules.Chat.Infrastructure.Database;

internal sealed class ChatMessageRepository(ChatDbContext dbContext) : IChatMessageRepository
{
    public void Insert(ChatMessage message)
    {
        dbContext.ChatMessages.Add(message);
    }
}
