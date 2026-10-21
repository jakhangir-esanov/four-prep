namespace FourPrep.Common.Application.Clock;

public interface ITimeZoneContext
{
    TimeZoneInfo TimeZone { get; }

    DateOnly Today();

    DateTime ToLocal(DateTime utc);
}
