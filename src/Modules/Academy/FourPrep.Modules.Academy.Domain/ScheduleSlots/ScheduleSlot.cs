namespace FourPrep.Modules.Academy.Domain.ScheduleSlots;

public sealed class ScheduleSlot
{
    public DayOfWeek Day { get; set; }
    public TimeOnly StartTime { get; set; }
    public int DurationMinutes { get; set; }

    public TimeOnly EndTime => StartTime.AddMinutes(DurationMinutes);

    public static ScheduleSlot Create(DayOfWeek day, TimeOnly startTime, int durationMinutes)
    {
        return new ScheduleSlot
        {
            Day = day,
            StartTime = startTime,
            DurationMinutes = durationMinutes
        };
    }

    public bool Overlaps(ScheduleSlot other) =>
        Day == other.Day && StartTime < other.EndTime && other.StartTime < EndTime;
}
