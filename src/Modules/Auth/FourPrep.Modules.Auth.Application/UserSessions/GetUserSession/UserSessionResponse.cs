namespace FourPrep.Modules.Auth.Application.UserSessions.GetUserSession;

public sealed record UserSessionResponse(
    Guid Id,
    Guid UserId,
    string AccessTokenHash,
    DateTime ExpiresOnUtc,
    bool IsRevoked,
    DateTime CreatedOnUtc,
    DateTime? RevokedOnUtc);
