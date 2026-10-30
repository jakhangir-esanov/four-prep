namespace FourPrep.Modules.Meeting.Application.StaffSchedules;

using FourPrep.Modules.Meeting.Domain.Meetings;
using FourPrep.Modules.Meeting.Application.Meetings;
using FourPrep.Modules.Meeting.Domain.StaffSchedules;
using FourPrep.Modules.Meeting.Domain.AvailabilityWindows;

public sealed record StaffScheduleDetails(
    string DisplayName,
    string TimeZone,
    string? MeetingUrl,
    string? OfficeCountryCode,
    string? OfficeCity,
    string? OfficeAddress,
    IReadOnlyList<AvailabilityWindowInput>? Windows)
{
    internal IReadOnlyList<AvailabilityWindowInput> GivenWindows => [.. (Windows ?? []).OfType<AvailabilityWindowInput>()];

    internal bool Offers(MeetingMode mode) => GivenWindows.Any(window => window.Mode == mode);

    internal StaffSchedule ToSchedule(Guid staffUserId, DateTime utcNow) =>
        StaffSchedule.Create(
            staffUserId,
            DisplayName.Trim(),
            TimeZone.Trim(),
            MeetingLinks.Normalize(MeetingUrl),
            OfficeCountryCode?.Trim().ToUpperInvariant(),
            OfficeCity?.Trim(),
            OfficeAddress?.Trim(),
            GivenWindows.Select(window => window.ToWindow()),
            utcNow);

    internal void ApplyTo(StaffSchedule schedule, DateTime utcNow) =>
        schedule.Update(
            DisplayName.Trim(),
            TimeZone.Trim(),
            MeetingLinks.Normalize(MeetingUrl),
            OfficeCountryCode?.Trim().ToUpperInvariant(),
            OfficeCity?.Trim(),
            OfficeAddress?.Trim(),
            GivenWindows.Select(window => window.ToWindow()),
            utcNow);
}

public sealed record AvailabilityWindowInput(DayOfWeek Day, TimeOnly StartTime, TimeOnly EndTime, MeetingMode Mode)
{
    internal AvailabilityWindow ToWindow() => AvailabilityWindow.Create(Day, StartTime, EndTime, Mode);
}
