namespace FourPrep.Common.Infrastructure.Clock;

using FourPrep.Common.Application.Clock;

internal sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
