namespace FourPrep.Modules.Auth.Application.Abstractions.Identity;

using FourPrep.Common.Domain.Localization;
using FourPrep.Modules.Auth.Application.Users.GetUser;

// Profile values kept on the identity account. Country, time zone and locale reach the access
// token as the country, zoneinfo and locale claims, so every module can read them without a lookup.
public sealed record UserAttributes(
    string? PhoneNumber,
    string? Country,
    string? TimeZone,
    string? Locale)
{
    public static readonly UserAttributes None = new(null, null, null, null);

    // Keycloak replaces every attribute on update, so an update starts from what the account already has.
    public static UserAttributes Of(UserResponse user) =>
        new(user.PhoneNumber, user.Country, user.TimeZone, user.Locale);

    public static UserAttributes Normalized(string? phoneNumber, string? country, string? timeZone, string? locale) =>
        new(
            PhoneNumbers.NormalizeOrNull(phoneNumber),
            Countries.NormalizeOrNull(country),
            string.IsNullOrWhiteSpace(timeZone) ? null : timeZone.Trim(),
            Locales.NormalizeOrNull(locale));
}
