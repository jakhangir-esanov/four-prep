namespace FourPrep.Modules.Auth.Presentation.Authentication.Requests;

internal sealed record LogoutRequest
{
    public string RefreshToken { get; init; }
}
