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

    public void Insert(UserSession userSession)
    {
        dbContext.UserSessions.Add(userSession);
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
