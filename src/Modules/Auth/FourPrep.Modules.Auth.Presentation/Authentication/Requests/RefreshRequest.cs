namespace FourPrep.Modules.Auth.Presentation.Authentication.Requests;

internal sealed record RefreshRequest
{
    public string RefreshToken { get; init; }
}
