namespace FourPrep.Modules.Catalogue.Application.Facts;

internal static class StaleFacts
{
    public const int MonthsUntilStale = 12;

    public static DateOnly CheckedBefore(DateTime utcNow) =>
        DateOnly.FromDateTime(utcNow).AddMonths(-MonthsUntilStale);
}
