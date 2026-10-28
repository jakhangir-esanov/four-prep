namespace FourPrep.Modules.Academy.Application.ClassGroups;

using FourPrep.Modules.Academy.Domain.ScheduleSlots;

public sealed record ScheduleSlotInput(DayOfWeek Day, TimeOnly StartTime, int DurationMinutes)
{
    internal ScheduleSlot ToSlot() => ScheduleSlot.Create(Day, StartTime, DurationMinutes);
}
