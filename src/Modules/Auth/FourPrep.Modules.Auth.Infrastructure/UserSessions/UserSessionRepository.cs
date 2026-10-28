namespace FourPrep.Modules.Auth.Infrastructure.UserSessions;

using Microsoft.EntityFrameworkCore;
using FourPrep.Modules.Auth.Domain.UserSessions;
using FourPrep.Modules.Auth.Infrastructure.Database;

internal sealed class UserSessionRepository(AuthDbContext dbContext) : IUserSessionRepository
{
    public async Task<UserSession?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.UserSessions.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<UserSession?> GetByIdentitySessionIdAsync(
        string identitySessionId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.UserSessions
            .Where(x => x.IdentitySessionId == identitySessionId)
            .OrderByDescending(x => x.CreatedOnUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public void Insert(UserSession userSession)
    {
        dbContext.UserSessions.Add(userSession);
    }

    public async Task RemoveAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        dbContext.UserSessions.RemoveRange(
            await dbContext.UserSessions.Where(x => x.UserId == userId).ToListAsync(cancellationToken));
    }

    public async Task<int> DeleteExpiredBeforeAsync(
        DateTime expiredBeforeUtc,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.UserSessions
            .Where(x => x.ExpiresOnUtc < expiredBeforeUtc)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task RevokeAllByUserIdAsync(
        Guid userId,
        DateTime revokedOnUtc,
        CancellationToken cancellationToken = default)
    {
        var sessions = await dbContext.UserSessions
            .Where(x => x.UserId == userId && !x.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var session in sessions)
        {
            session.Revoke(revokedOnUtc);
        }
    }
}
