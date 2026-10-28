namespace FourPrep.Common.Application.Data;

public static class LikePatterns
{
    public static string? Contains(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return null;
        }

        string escaped = search.Trim()
            .Replace(@"\", @"\\")
            .Replace("%", @"\%")
            .Replace("_", @"\_");

        return $"%{escaped}%";
    }
}
