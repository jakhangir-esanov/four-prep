namespace FourPrep.Common.Application.Extensions;

using FluentValidation;
using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Domain.Localization;

public static class RuleBuilderExtensions
{
    public static IRuleBuilderOptions<T, TProperty> WithError<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> rule,
        Error error)
    {
        return rule
            .WithErrorCode(error.Code)
            .WithMessage(error.Message)
            .WithState(_ => error);
    }

    // The rules below let an empty value through, so they fit optional fields; add NotEmpty() where one is required.
    public static IRuleBuilderOptions<T, string?> CountryCode<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(code => string.IsNullOrWhiteSpace(code) || Countries.IsValid(code))
            .WithError(LocalizationErrors.InvalidCountry);

    public static IRuleBuilderOptions<T, string?> TimeZoneId<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(ianaId => string.IsNullOrWhiteSpace(ianaId) || TimeZones.IsValid(ianaId))
            .WithError(LocalizationErrors.InvalidTimeZone);

    public static IRuleBuilderOptions<T, string?> Locale<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(locale => string.IsNullOrWhiteSpace(locale) || Locales.IsSupported(locale))
            .WithError(LocalizationErrors.UnsupportedLocale);

    public static IRuleBuilderOptions<T, string?> PhoneNumber<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(phone => string.IsNullOrWhiteSpace(phone) || PhoneNumbers.IsValid(phone))
            .WithError(LocalizationErrors.InvalidPhoneNumber);
}
