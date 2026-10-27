namespace FourPrep.Common.Domain.Localization;

// Languages the platform has messages for. They match LocalizedMessage and the Keycloak realm.
public static class Locales
{
    public const string English = "en";
    public const string Uzbek = "uz";
    public const string Russian = "ru";

    public const string Default = English;

    public static readonly IReadOnlyList<string> All = [English, Uzbek, Russian];

    public static bool IsSupported(string? locale) =>
        locale is not null && All.Contains(Normalize(locale));

    public static string Normalize(string locale) => locale.Trim().ToLowerInvariant();

    public static string? NormalizeOrNull(string? locale) =>
        string.IsNullOrWhiteSpace(locale) ? null : Normalize(locale);
}
