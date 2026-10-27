namespace FourPrep.Modules.Auth.Presentation.Account.Requests;

internal sealed record UpdateMyAccountRequest
{
    public string? PhoneNumber { get; init; }
    public string Country { get; init; }
    public string? TimeZone { get; init; }
    public string? Locale { get; init; }
}
