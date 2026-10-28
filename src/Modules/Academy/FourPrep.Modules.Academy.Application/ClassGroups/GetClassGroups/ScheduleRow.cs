namespace FourPrep.Modules.Academy.Application.ClassGroups.GetClassGroups;

internal sealed class ScheduleRow
{
    public Guid ClassGroupId { get; init; }
    public DayOfWeek Day { get; init; }
    public TimeOnly StartTime { get; init; }
    public int DurationMinutes { get; init; }

    public ScheduleSlotResponse ToResponse() =>
        new()
        {
            Day = Day,
            StartTime = StartTime,
            DurationMinutes = DurationMinutes
        };
}
