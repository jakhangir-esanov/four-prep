namespace FourPrep.Modules.Meeting.Domain.StaffSchedules;

using FourPrep.Modules.Meeting.Domain.Meetings;
using FourPrep.Modules.Meeting.Domain.AvailabilityWindows;

public sealed class StaffSchedule
{
    private readonly List<AvailabilityWindow> _windows = [];

    public Guid Id { get; set; }
    public Guid StaffUserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string TimeZone { get; set; } = string.Empty;
    public string? MeetingUrl { get; set; }
    public string? OfficeCountryCode { get; set; }
    public string? OfficeCity { get; set; }
    public string? OfficeAddress { get; set; }
    public IReadOnlyCollection<AvailabilityWindow> Windows => _windows;
    public DateTimeOffset? LastBookedOnUtc { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset UpdatedOnUtc { get; set; }

    public static StaffSchedule Create(
        Guid staffUserId,
        string displayName,
        string timeZone,
        string? meetingUrl,
        string? officeCountryCode,
        string? officeCity,
        string? officeAddress,
        IEnumerable<AvailabilityWindow> windows,
        DateTime utcNow)
    {
        var schedule = new StaffSchedule
        {
            Id = Guid.NewGuid(),
            StaffUserId = staffUserId,
            CreatedOnUtc = utcNow
        };

        schedule.Update(
            displayName,
            timeZone,
            meetingUrl,
            officeCountryCode,
            officeCity,
            officeAddress,
            windows,
            utcNow);

        return schedule;
    }

    public void Update(
        string displayName,
        string timeZone,
        string? meetingUrl,
        string? officeCountryCode,
        string? officeCity,
        string? officeAddress,
        IEnumerable<AvailabilityWindow> windows,
        DateTime utcNow)
    {
        DisplayName = displayName;
        TimeZone = timeZone;
        MeetingUrl = meetingUrl;
        OfficeCountryCode = officeCountryCode;
        OfficeCity = officeCity;
        OfficeAddress = officeAddress;
        _windows.Clear();
        _windows.AddRange(windows);
        UpdatedOnUtc = utcNow;
    }

    public bool HasMode(MeetingMode mode) => _windows.Exists(window => window.Mode == mode);

    public void RecordBooking(DateTime utcNow)
    {
        LastBookedOnUtc = utcNow;
    }
}
