namespace FourPrep.Modules.Auth.Application.Authentication;

public sealed record AuthTokenResponse(
    string AccessToken,
    string RefreshToken,
    string? IdToken,
    string TokenType,
    int ExpiresIn,
    int RefreshExpiresIn);
