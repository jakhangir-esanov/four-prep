namespace FourPrep.Modules.Auth.Domain.UserSessions;

public sealed class UserSession
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string AccessTokenHash { get; set; } = null!;
    public DateTimeOffset ExpiresOnUtc { get; set; }
    public bool IsRevoked { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? RevokedOnUtc { get; set; }

    public static UserSession Create(
        Guid userId,
        string accessTokenHash,
        DateTime expiresOnUtc,
        DateTime createdOnUtc)
    {
        return new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AccessTokenHash = accessTokenHash,
            ExpiresOnUtc = expiresOnUtc,
            IsRevoked = false,
            CreatedOnUtc = createdOnUtc
        };
    }

    public void Revoke(DateTime revokedOnUtc)
    {
        IsRevoked = true;
        RevokedOnUtc = revokedOnUtc;
    }
}
