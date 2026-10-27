namespace FourPrep.Common.Domain.Localization;

using System.Text.RegularExpressions;

public static partial class PhoneNumbers
{
    public static bool IsValid(string? phoneNumber) =>
        phoneNumber is not null && E164Pattern().IsMatch(Normalize(phoneNumber));

    public static string Normalize(string phoneNumber) =>
        SeparatorPattern().Replace(phoneNumber.Trim(), string.Empty);

    public static string? NormalizeOrNull(string? phoneNumber) =>
        string.IsNullOrWhiteSpace(phoneNumber) ? null : Normalize(phoneNumber);

    [GeneratedRegex(@"^\+[1-9]\d{6,14}$")]
    private static partial Regex E164Pattern();

    [GeneratedRegex(@"[\s\-().]")]
    private static partial Regex SeparatorPattern();
}
