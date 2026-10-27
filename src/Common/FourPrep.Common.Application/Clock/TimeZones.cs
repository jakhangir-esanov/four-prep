namespace FourPrep.Common.Application.Clock;

public static class TimeZones
{
    // Used when a user has not told us their time zone (and for jobs that run outside a request).
    public const string DefaultIanaId = "UTC";

    public static readonly TimeZoneInfo Default = TimeZoneInfo.Utc;

    public static bool IsValid(string? ianaId) =>
        !string.IsNullOrWhiteSpace(ianaId) && TryFind(ianaId, out _);

    public static bool TryFind(string? ianaId, out TimeZoneInfo timeZone)
    {
        timeZone = Default;

        if (string.IsNullOrWhiteSpace(ianaId))
        {
            return false;
        }

        if (string.Equals(ianaId, DefaultIanaId, StringComparison.Ordinal))
        {
            return true;
        }

        // Windows names ("Pacific Standard Time") would resolve too; accept IANA names only,
        // so the web, mobile apps and the token all spell a zone the same way.
        if (!TimeZoneInfo.TryConvertIanaIdToWindowsId(ianaId, out _) ||
            !TimeZoneInfo.TryFindSystemTimeZoneById(ianaId, out TimeZoneInfo? found))
        {
            return false;
        }

        timeZone = found;
        return true;
    }

    public static DateTime ToLocal(DateTime utc, TimeZoneInfo timeZone) =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), timeZone);

    public static DateOnly ToLocalDate(DateTime utc, TimeZoneInfo timeZone) =>
        DateOnly.FromDateTime(ToLocal(utc, timeZone));
}
