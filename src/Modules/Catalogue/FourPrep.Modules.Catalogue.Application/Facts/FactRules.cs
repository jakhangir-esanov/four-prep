namespace FourPrep.Modules.Catalogue.Application.Facts;

using FluentValidation;
using FourPrep.Common.Application.Clock;
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

    public static bool IsWebAddress(string? value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
}
