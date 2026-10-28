namespace FourPrep.Modules.Auth.Domain.UserSessions;

public interface IUserSessionRepository
{
    Task<UserSession?> GetAsync(Guid id, CancellationToken cancellationToken = default!);
    Task<UserSession?> GetByIdentitySessionIdAsync(string identitySessionId, CancellationToken cancellationToken = default!);
    void Insert(UserSession userSession);
    Task RevokeAllByUserIdAsync(Guid userId, DateTime revokedOnUtc, CancellationToken cancellationToken = default!);
}
