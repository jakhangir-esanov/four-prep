namespace FourPrep.Common.Infrastructure.Clock;

using FourPrep.Common.Application.Clock;

internal sealed class TimeZoneContext(IDateTimeProvider dateTimeProvider) : ITimeZoneContext
{
    public TimeZoneInfo TimeZone => TimeZones.Business;

    public DateOnly Today() => TimeZones.ToLocalDate(dateTimeProvider.UtcNow, TimeZone);

    public DateTime ToLocal(DateTime utc) => TimeZones.ToLocal(utc, TimeZone);
}
