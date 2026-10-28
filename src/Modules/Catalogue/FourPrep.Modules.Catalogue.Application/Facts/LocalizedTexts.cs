namespace FourPrep.Modules.Catalogue.Application.Facts;

using FourPrep.Common.Domain.Localization;

internal static class LocalizedTexts
{
    public static Dictionary<string, string> Normalize(IReadOnlyDictionary<string, string>? text) =>
        (text ?? new Dictionary<string, string>())
            .Where(entry => !string.IsNullOrWhiteSpace(entry.Key) && !string.IsNullOrWhiteSpace(entry.Value))
            .GroupBy(entry => Locales.Normalize(entry.Key))
            .ToDictionary(group => group.Key, group => group.Last().Value.Trim());
}
