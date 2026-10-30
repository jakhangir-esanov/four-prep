namespace FourPrep.Modules.Meeting.Application.Meetings;

internal static class UtcTimes
{
    public static DateTime From(DateTime value) => value.Kind switch
    {
        DateTimeKind.Local => value.ToUniversalTime(),
        DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
        _ => value
    };

    public static DateTime? From(DateTime? value) => value is null ? null : From(value.Value);
}
