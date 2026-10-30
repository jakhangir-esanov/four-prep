namespace FourPrep.Modules.Meeting.Domain.AvailabilityWindows;

using FourPrep.Modules.Meeting.Domain.Meetings;

public sealed class AvailabilityWindow
{
    public DayOfWeek Day { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public MeetingMode Mode { get; set; }

    public static AvailabilityWindow Create(DayOfWeek day, TimeOnly startTime, TimeOnly endTime, MeetingMode mode)
    {
        return new AvailabilityWindow
        {
            Day = day,
            StartTime = startTime,
            EndTime = endTime,
            Mode = mode
        };
    }

    public bool Overlaps(AvailabilityWindow other) =>
        Day == other.Day && Mode == other.Mode && StartTime < other.EndTime && other.StartTime < EndTime;
}
