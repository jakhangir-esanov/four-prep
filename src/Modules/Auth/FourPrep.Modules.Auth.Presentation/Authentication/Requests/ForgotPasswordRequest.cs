namespace FourPrep.Modules.Auth.Presentation.Authentication.Requests;

internal sealed record ForgotPasswordRequest
{
    public string Email { get; init; }
}
