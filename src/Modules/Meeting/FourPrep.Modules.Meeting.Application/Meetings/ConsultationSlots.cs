namespace FourPrep.Modules.Meeting.Application.Meetings;

using FourPrep.Modules.Meeting.Domain.Meetings;

internal static class ConsultationSlots
{
    public static readonly TimeSpan Step = TimeSpan.FromMinutes(30);

    public static readonly TimeSpan MinNotice = TimeSpan.FromHours(12);

    public static readonly TimeSpan MaxAhead = TimeSpan.FromDays(30);

    public static readonly TimeSpan Duration = TimeSpan.FromMinutes(Meeting.ConsultationMinutes);

    public static IEnumerable<OfferedTime> Offered(
        string timeZoneId,
        IReadOnlyCollection<WeeklyWindow> windows,
        DateTime fromUtc,
        DateTime toUtc)
    {
        var zone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        var firstDay = LocalDate(fromUtc, zone).AddDays(-1);
        var lastDay = LocalDate(toUtc, zone).AddDays(1);

        for (var day = firstDay; day <= lastDay; day = day.AddDays(1))
        {
            foreach (var window in windows.Where(window => window.Day == day.DayOfWeek))
            {
                foreach (var startsOnUtc in StartsInWindow(day, window, zone))
                {
                    if (startsOnUtc >= fromUtc && startsOnUtc < toUtc)
                    {
                        yield return new OfferedTime(startsOnUtc, window.Mode);
                    }
                }
            }
        }
    }

    public static bool IsOffered(
        string timeZoneId,
        IReadOnlyCollection<WeeklyWindow> windows,
        MeetingMode mode,
        DateTime startsOnUtc) =>
        Offered(timeZoneId, windows, startsOnUtc, startsOnUtc.AddTicks(1))
            .Any(offered => offered.Mode == mode && offered.StartsOnUtc == startsOnUtc);

    public static bool IsBookableAt(DateTime startsOnUtc, DateTime utcNow) =>
        startsOnUtc >= utcNow + MinNotice && startsOnUtc <= utcNow + MaxAhead;

    private static IEnumerable<DateTime> StartsInWindow(DateOnly day, WeeklyWindow window, TimeZoneInfo zone)
    {
        var end = window.EndTime.ToTimeSpan();

        for (var start = window.StartTime.ToTimeSpan(); start + Duration <= end; start += Step)
        {
            var local = day.ToDateTime(TimeOnly.FromTimeSpan(start), DateTimeKind.Unspecified);
            if (!zone.IsInvalidTime(local))
            {
                yield return TimeZoneInfo.ConvertTimeToUtc(local, zone);
            }
        }
    }

    private static DateOnly LocalDate(DateTime utc, TimeZoneInfo zone) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), zone));
}

internal sealed record WeeklyWindow(DayOfWeek Day, TimeOnly StartTime, TimeOnly EndTime, MeetingMode Mode);

internal sealed record OfferedTime(DateTime StartsOnUtc, MeetingMode Mode);
