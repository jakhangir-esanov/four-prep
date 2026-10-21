namespace FourPrep.Common.Application.Clock;

public static class TimeZones
{
    public const string BusinessIanaId = "Asia/Tashkent";

    public static readonly TimeZoneInfo Business = TimeZoneInfo.FindSystemTimeZoneById(BusinessIanaId);

    public static DateTime ToLocal(DateTime utc, TimeZoneInfo timeZone) =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), timeZone);

    public static DateOnly ToLocalDate(DateTime utc, TimeZoneInfo timeZone) =>
        DateOnly.FromDateTime(ToLocal(utc, timeZone));
}
