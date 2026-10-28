namespace FourPrep.Modules.Academy.Presentation.ClassGroups.Requests;

using FourPrep.Modules.Academy.Application.ClassGroups;
using FourPrep.Modules.Academy.Domain.ClassGroups;

internal sealed record ClassGroupRequest
{
    public string? Code { get; init; }
    public ClassMode Mode { get; init; }
    public string? CountryCode { get; init; }
    public string? City { get; init; }
    public string? Address { get; init; }
    public Dictionary<string, string>? EntryLevel { get; init; }
    public string? TimeZone { get; init; }
    public DateOnly StartsOn { get; init; }
    public int Weeks { get; init; }
    public int Capacity { get; init; }
    public IReadOnlyList<ScheduleSlotRequest>? Schedule { get; init; }

    public ClassGroupDetails ToDetails() =>
        new(
            Code ?? string.Empty,
            Mode,
            CountryCode,
            City,
            Address,
            EntryLevel,
            TimeZone ?? string.Empty,
            StartsOn,
            Weeks,
            Capacity,
            Schedule?
                .OfType<ScheduleSlotRequest>()
                .Select(slot => new ScheduleSlotInput(slot.Day, slot.StartTime, slot.DurationMinutes))
                .ToList());
}

internal sealed record ScheduleSlotRequest
{
    public DayOfWeek Day { get; init; }
    public TimeOnly StartTime { get; init; }
    public int DurationMinutes { get; init; }
}
