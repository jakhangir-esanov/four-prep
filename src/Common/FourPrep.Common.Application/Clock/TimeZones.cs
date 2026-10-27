namespace FourPrep.Common.Application.Clock;

public static class TimeZones
{
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

        if (!IsIanaId(ianaId) ||
            !TimeZoneInfo.TryFindSystemTimeZoneById(ianaId, out TimeZoneInfo? found))
        {
            return false;
        }

        timeZone = found;
        return true;
    }

    private static bool IsIanaId(string id) =>
        TimeZoneInfo.TryConvertIanaIdToWindowsId(id, out _);

    public static DateTime ToLocal(DateTime utc, TimeZoneInfo timeZone) =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), timeZone);

    public static DateOnly ToLocalDate(DateTime utc, TimeZoneInfo timeZone) =>
        DateOnly.FromDateTime(ToLocal(utc, timeZone));
}
