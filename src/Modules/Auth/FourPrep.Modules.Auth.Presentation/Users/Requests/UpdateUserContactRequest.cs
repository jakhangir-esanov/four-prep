namespace FourPrep.Modules.Auth.Presentation.Users.Requests;

internal sealed record UpdateUserContactRequest
{
    public string Email { get; init; }
    public string? PhoneNumber { get; init; }
}
