namespace FourPrep.Modules.Auth.Domain.UserSessions;

public sealed class UserSession
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? IdentitySessionId { get; set; }
    public string AccessTokenHash { get; set; } = null!;
    public DateTimeOffset ExpiresOnUtc { get; set; }
    public bool IsRevoked { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? RefreshedOnUtc { get; set; }
    public DateTimeOffset? RevokedOnUtc { get; set; }

    public static UserSession Create(
        Guid userId,
        string? identitySessionId,
        string accessTokenHash,
        DateTime expiresOnUtc,
        DateTime createdOnUtc)
    {
        return new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            IdentitySessionId = identitySessionId,
            AccessTokenHash = accessTokenHash,
            ExpiresOnUtc = expiresOnUtc,
            IsRevoked = false,
            CreatedOnUtc = createdOnUtc
        };
    }

    public void Refresh(string accessTokenHash, DateTime expiresOnUtc, DateTime refreshedOnUtc)
    {
        AccessTokenHash = accessTokenHash;
        ExpiresOnUtc = expiresOnUtc;
        RefreshedOnUtc = refreshedOnUtc;
    }

    public void Revoke(DateTime revokedOnUtc)
    {
        IsRevoked = true;
        RevokedOnUtc = revokedOnUtc;
    }
}
