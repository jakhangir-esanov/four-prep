namespace FourPrep.Modules.Meeting.Application.StaffSchedules.GetMySchedule;

using FourPrep.Modules.Meeting.Domain.Meetings;

public sealed record StaffScheduleResponse
{
    public Guid Id { get; init; }
    public Guid StaffUserId { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string TimeZone { get; init; } = string.Empty;
    public string? MeetingUrl { get; init; }
    public string? OfficeCountryCode { get; init; }
    public string? OfficeCity { get; init; }
    public string? OfficeAddress { get; init; }
    public IReadOnlyList<AvailabilityWindowResponse> Windows { get; init; } = [];
    public DateTime UpdatedOnUtc { get; init; }
}

public sealed record AvailabilityWindowResponse
{
    public Guid StaffScheduleId { get; init; }
    public DayOfWeek Day { get; init; }
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }
    public MeetingMode Mode { get; init; }
}
