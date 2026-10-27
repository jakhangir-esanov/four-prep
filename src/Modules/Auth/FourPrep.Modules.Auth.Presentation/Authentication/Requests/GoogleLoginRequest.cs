namespace FourPrep.Modules.Auth.Presentation.Authentication.Requests;

internal sealed record GoogleLoginRequest
{
    public string IdToken { get; init; }
    public string? Role { get; init; }
    public string? Country { get; init; }
    public string? TimeZone { get; init; }
    public string? Locale { get; init; }
}
