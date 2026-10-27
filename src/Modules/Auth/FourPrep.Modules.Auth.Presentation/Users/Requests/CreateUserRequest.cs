namespace FourPrep.Modules.Auth.Presentation.Users.Requests;

internal sealed record CreateUserRequest
{
    public string Username { get; init; }
    public string Email { get; init; }
    public string? PhoneNumber { get; init; }
    public string Password { get; init; }
    public string Role { get; init; }
    public string? Country { get; init; }
    public string? TimeZone { get; init; }
    public string? Locale { get; init; }
}
