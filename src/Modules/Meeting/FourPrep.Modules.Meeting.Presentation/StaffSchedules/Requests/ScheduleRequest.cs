namespace FourPrep.Modules.Meeting.Presentation.StaffSchedules.Requests;

using FourPrep.Modules.Meeting.Domain.Meetings;
using FourPrep.Modules.Meeting.Application.StaffSchedules;

internal sealed record ScheduleRequest
{
    public string? DisplayName { get; init; }
    public string? TimeZone { get; init; }
    public string? MeetingUrl { get; init; }
    public string? OfficeCountryCode { get; init; }
    public string? OfficeCity { get; init; }
    public string? OfficeAddress { get; init; }
    public List<AvailabilityWindowRequest>? Windows { get; init; }

    public StaffScheduleDetails ToDetails() =>
        new(
            DisplayName ?? string.Empty,
            TimeZone ?? string.Empty,
            MeetingUrl,
            OfficeCountryCode,
            OfficeCity,
            OfficeAddress,
            Windows?.Select(window => window.ToInput()).ToList());
}

internal sealed record AvailabilityWindowRequest
{
    public DayOfWeek Day { get; init; }
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }
    public MeetingMode Mode { get; init; }

    public AvailabilityWindowInput ToInput() => new(Day, StartTime, EndTime, Mode);
}
