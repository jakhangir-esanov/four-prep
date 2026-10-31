namespace FourPrep.Modules.Chat.Infrastructure.ChatThreads;

using Microsoft.EntityFrameworkCore;
using FourPrep.Modules.Chat.Domain.ChatThreads;
using FourPrep.Modules.Chat.Infrastructure.Database;

internal sealed class ChatThreadRepository(ChatDbContext dbContext) : IChatThreadRepository
{
    public async Task<ChatThread?> GetAsync(
        Guid studentUserId,
        ChatThreadKind kind,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.ChatThreads.SingleOrDefaultAsync(
            x => x.StudentUserId == studentUserId && x.Kind == kind,
            cancellationToken);
    }

    public void Insert(ChatThread thread)
    {
        dbContext.ChatThreads.Add(thread);
    }
}
