namespace FourPrep.Modules.Auth.Presentation.Authentication.Requests;

internal sealed record LoginRequest
{
    public string Username { get; init; }
    public string Password { get; init; }
}
