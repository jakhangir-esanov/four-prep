namespace FourPrep.Modules.Auth.Presentation.Authentication.Requests;

internal sealed record RegisterRequest
{
    public string Username { get; init; }
    public string Email { get; init; }
    public string? PhoneNumber { get; init; }
    public string Password { get; init; }

    // ISO 3166-1 alpha-2, for example "US".
    public string Country { get; init; }

    // IANA name, for example "America/New_York"; the browser's Intl API gives it.
    public string? TimeZone { get; init; }

    // One of "en", "uz", "ru"; emails from the platform use it.
    public string? Locale { get; init; }
}
