namespace FourPrep.Modules.Catalogue.Application.Facts;

internal static class TextInput
{
    public static string? TrimmedOrNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
