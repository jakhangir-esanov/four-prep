namespace FourPrep.Modules.Auth.Application.Abstractions.Identity;

using FourPrep.Common.Domain.Localization;
using FourPrep.Modules.Auth.Application.Users.GetUser;

public sealed record UserAttributes(
    string? PhoneNumber,
    string? Country,
    string? TimeZone,
    string? Locale)
{
    public static readonly UserAttributes None = new(null, null, null, null);

    public static UserAttributes Of(UserResponse user) =>
        new(user.PhoneNumber, user.Country, user.TimeZone, user.Locale);

    public static UserAttributes Normalized(string? phoneNumber, string? country, string? timeZone, string? locale) =>
        new(
            PhoneNumbers.NormalizeOrNull(phoneNumber),
            Countries.NormalizeOrNull(country),
            string.IsNullOrWhiteSpace(timeZone) ? null : timeZone.Trim(),
            Locales.NormalizeOrNull(locale));
}
