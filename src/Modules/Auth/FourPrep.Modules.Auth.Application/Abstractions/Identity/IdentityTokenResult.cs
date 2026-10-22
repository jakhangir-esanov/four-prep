namespace FourPrep.Modules.Auth.Application.Abstractions.Identity;

public sealed record IdentityTokenResult(
    string AccessToken,
    string RefreshToken,
    string? IdToken,
    string TokenType,
    int ExpiresIn,
    int RefreshExpiresIn,
    Guid UserId,
    DateTime ExpiresAt);
