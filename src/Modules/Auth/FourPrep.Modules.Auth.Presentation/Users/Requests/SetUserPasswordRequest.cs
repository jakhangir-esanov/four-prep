namespace FourPrep.Modules.Auth.Presentation.Users.Requests;

internal sealed record SetUserPasswordRequest
{
    public string Password { get; init; }
}
