namespace FourPrep.Modules.Catalogue.Application.Facts;

using FluentValidation;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Domain.Localization;
using FourPrep.Common.Application.Extensions;
using FourPrep.Modules.Catalogue.Domain.Facts;

internal static class FactRules
{
    public const int MaxUrlLength = 500;

    public static IRuleBuilderOptions<T, string> SourceUrl<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty()
            .MaximumLength(MaxUrlLength)
            .Must(url => string.IsNullOrWhiteSpace(url) || IsWebAddress(url))
            .WithError(FactErrors.InvalidSourceUrl);

    public static IRuleBuilderOptions<T, DateOnly> CheckedOn<T>(
        this IRuleBuilder<T, DateOnly> rule,
        ITimeZoneContext timeZoneContext) =>
        rule.NotEmpty()
            .Must(checkedOn => checkedOn <= timeZoneContext.Today())
            .WithError(FactErrors.CheckedOnInFuture);

    public static IRuleBuilderOptions<T, IReadOnlyDictionary<string, string>?> LocalizedText<T>(
        this IRuleBuilder<T, IReadOnlyDictionary<string, string>?> rule,
        int maxLength) =>
        rule.Must(text => text is null || text.Keys.All(Locales.IsSupported))
            .WithError(LocalizationErrors.UnsupportedLocale)
            .Must(text => text is null || text.Values.All(value => value is null || value.Trim().Length <= maxLength))
            .Must(HasEnglishWhenTranslated)
            .WithError(FactErrors.MissingEnglishText);

    public static bool IsWebAddress(string? value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);

    private static bool HasEnglishWhenTranslated(IReadOnlyDictionary<string, string>? text)
    {
        var translated = LocalizedTexts.Normalize(text);

        return translated.Count == 0 || translated.ContainsKey(Locales.English);
    }
}
