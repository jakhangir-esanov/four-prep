namespace FourPrep.Common.Infrastructure.Clock;

using Microsoft.AspNetCore.Http;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Infrastructure.Authentication;

internal sealed class TimeZoneContext(
    IDateTimeProvider dateTimeProvider,
    IHttpContextAccessor httpContextAccessor) : ITimeZoneContext
{
    public TimeZoneInfo TimeZone =>
        TimeZones.TryFind(httpContextAccessor.HttpContext?.User.GetTimeZoneOrNull(), out var timeZone)
            ? timeZone
            : TimeZones.Default;

    public DateOnly Today() => TimeZones.ToLocalDate(dateTimeProvider.UtcNow, TimeZone);

    public DateTime ToLocal(DateTime utc) => TimeZones.ToLocal(utc, TimeZone);
}
