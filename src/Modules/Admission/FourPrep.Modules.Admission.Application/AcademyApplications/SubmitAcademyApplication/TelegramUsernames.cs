namespace FourPrep.Modules.Admission.Application.AcademyApplications.SubmitAcademyApplication;

using System.Text.RegularExpressions;

internal static partial class TelegramUsernames
{
    public static bool IsValid(string? username) =>
        username is not null && Pattern().IsMatch(Normalize(username));

    public static string? NormalizeOrNull(string? username) =>
        string.IsNullOrWhiteSpace(username) ? null : Normalize(username);

    private static string Normalize(string username) => username.Trim().TrimStart('@');

    [GeneratedRegex("^[A-Za-z][A-Za-z0-9_]{4,31}$")]
    private static partial Regex Pattern();
}
