namespace FourPrep.Modules.Chat.Infrastructure.ChatReadMarks;

using Microsoft.EntityFrameworkCore;
using FourPrep.Modules.Chat.Domain.ChatReadMarks;
using FourPrep.Modules.Chat.Infrastructure.Database;

internal sealed class ChatReadMarkRepository(ChatDbContext dbContext) : IChatReadMarkRepository
{
    public async Task<ChatReadMark?> GetAsync(Guid threadId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.ChatReadMarks.SingleOrDefaultAsync(
            x => x.ThreadId == threadId && x.UserId == userId,
            cancellationToken);
    }

    public void Insert(ChatReadMark readMark)
    {
        dbContext.ChatReadMarks.Add(readMark);
    }
}
