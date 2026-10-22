namespace FourPrep.Modules.Auth.Presentation.Authentication.Requests;

internal sealed record RegisterRequest
{
    public string Username { get; init; }
    public string Email { get; init; }
    public string? PhoneNumber { get; init; }
    public string Password { get; init; }
}
