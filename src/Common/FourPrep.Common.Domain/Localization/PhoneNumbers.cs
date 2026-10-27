namespace FourPrep.Common.Domain.Localization;

using System.Text.RegularExpressions;

// Phone numbers are kept in E.164 form (+ country code + number, at most 15 digits),
// so numbers from any country compare and search the same way.
public static partial class PhoneNumbers
{
    public static bool IsValid(string? phoneNumber) =>
        phoneNumber is not null && E164Pattern().IsMatch(Normalize(phoneNumber));

    // Drops the separators people type ("+1 (415) 555-0100" becomes "+14155550100").
    public static string Normalize(string phoneNumber) =>
        SeparatorPattern().Replace(phoneNumber.Trim(), string.Empty);

    public static string? NormalizeOrNull(string? phoneNumber) =>
        string.IsNullOrWhiteSpace(phoneNumber) ? null : Normalize(phoneNumber);

    [GeneratedRegex(@"^\+[1-9]\d{6,14}$")]
    private static partial Regex E164Pattern();

    [GeneratedRegex(@"[\s\-().]")]
    private static partial Regex SeparatorPattern();
}
