namespace FourPrep.Modules.Auth.Application.Authentication;

using FourPrep.Modules.Auth.Application.Abstractions.Identity;

public sealed record AuthTokenResponse(
    string AccessToken,
    string RefreshToken,
    string? IdToken,
    string TokenType,
    int ExpiresIn,
    int RefreshExpiresIn)
{
    internal static AuthTokenResponse From(IdentityTokenResult tokens) =>
        new(
            tokens.AccessToken,
            tokens.RefreshToken,
            tokens.IdToken,
            tokens.TokenType,
            tokens.ExpiresIn,
            tokens.RefreshExpiresIn);
}
