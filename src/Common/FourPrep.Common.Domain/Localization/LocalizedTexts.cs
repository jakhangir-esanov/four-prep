namespace FourPrep.Common.Domain.Localization;

public static class LocalizedTexts
{
    public static Dictionary<string, string> Normalize(IReadOnlyDictionary<string, string>? text) =>
        (text ?? new Dictionary<string, string>())
            .Where(entry => !string.IsNullOrWhiteSpace(entry.Key) && !string.IsNullOrWhiteSpace(entry.Value))
            .GroupBy(entry => Locales.Normalize(entry.Key))
            .ToDictionary(group => group.Key, group => group.Last().Value.Trim());

    public static bool HasEnglishWhenTranslated(IReadOnlyDictionary<string, string>? text)
    {
        var translated = Normalize(text);

        return translated.Count == 0 || translated.ContainsKey(Locales.English);
    }

    public static bool HasEnglish(IReadOnlyDictionary<string, string>? text) =>
        Normalize(text).ContainsKey(Locales.English);
}
