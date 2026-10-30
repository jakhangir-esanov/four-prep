namespace FourPrep.Modules.Meeting.UnitTests.Meetings;

using FourPrep.Modules.Meeting.Domain.Meetings;
using FourPrep.Modules.Meeting.Application.Meetings;
using FourPrep.Modules.Meeting.Application.StaffSchedules.GetMySchedule;
using FourPrep.Modules.Meeting.Application.Meetings.GetConsultationSlots;

public class ConsultationSlotsTests
{
    private static readonly Guid StaffUserId = Guid.NewGuid();

    private static readonly DateTime Monday = ScheduleSamples.MondayNineInNewYorkUtc;

    [Fact]
    public void Offered_StartsEveryHalfHourInTheStaffTimeZoneAndFitsInTheWindow()
    {
        var offered = ConsultationSlots
            .Offered("America/New_York", Windows(), Monday.AddHours(-1), Monday.AddHours(4))
            .Where(slot => slot.Mode == MeetingMode.Online)
            .Select(slot => slot.StartsOnUtc);

        Assert.Equal([Monday, Monday.AddMinutes(30), Monday.AddMinutes(60)], offered);
    }

    [Fact]
    public void Offered_FollowsDaylightSavingTimeInTheStaffTimeZone()
    {
        var mondayBeforeTheClockChange = new DateTime(2026, 10, 26, 13, 0, 0, DateTimeKind.Utc);

        var first = ConsultationSlots
            .Offered("America/New_York", Windows(), mondayBeforeTheClockChange.AddHours(-1), mondayBeforeTheClockChange.AddHours(4))
            .First(slot => slot.Mode == MeetingMode.Online);

        Assert.Equal(mondayBeforeTheClockChange, first.StartsOnUtc);
    }

    [Fact]
    public void Offered_KeepsOnlyTimesInsideTheRange()
    {
        var offered = ConsultationSlots
            .Offered("America/New_York", Windows(), Monday.AddMinutes(30), Monday.AddMinutes(60))
            .Select(slot => slot.StartsOnUtc)
            .Distinct();

        Assert.Equal([Monday.AddMinutes(30)], offered);
    }

    [Fact]
    public void IsOffered_ChecksTheModeOfTheWindow()
    {
        Assert.True(ConsultationSlots.IsOffered("America/New_York", Windows(), MeetingMode.Phone, Monday));
        Assert.False(ConsultationSlots.IsOffered("America/New_York", Windows(), MeetingMode.Phone, Monday.AddMinutes(30)));
        Assert.False(ConsultationSlots.IsOffered("America/New_York", Windows(), MeetingMode.Office, Monday));
        Assert.False(ConsultationSlots.IsOffered("America/New_York", Windows(), MeetingMode.Online, Monday.AddMinutes(15)));
    }

    [Fact]
    public void IsBookableAt_NeedsTwelveHoursNoticeAndAtMostThirtyDays()
    {
        var now = new FakeClock().UtcNow;

        Assert.False(ConsultationSlots.IsBookableAt(now.AddHours(11), now));
        Assert.True(ConsultationSlots.IsBookableAt(now.AddHours(12), now));
        Assert.True(ConsultationSlots.IsBookableAt(now.AddDays(30), now));
        Assert.False(ConsultationSlots.IsBookableAt(now.AddDays(31), now));
    }

    [Fact]
    public void FreeSlots_GroupsModesAndLeavesOutTimesThatOverlapABooking()
    {
        var booked = new[]
        {
            new BookedTime { StaffUserId = StaffUserId, StartsOnUtc = Monday.AddMinutes(60), DurationMinutes = 45 }
        }.ToLookup(time => time.StaffUserId);

        var slots = GetConsultationSlotsQueryHandler.FreeSlots([Schedule()], booked, null, Monday.AddHours(-1), Monday.AddHours(4));

        var slot = Assert.Single(slots);
        Assert.Equal(Monday, slot.StartsOnUtc);
        Assert.Equal([MeetingMode.Online, MeetingMode.Phone], slot.Modes);
        Assert.Equal("Ms Rivera", slot.StaffName);
    }

    [Fact]
    public void FreeSlots_FiltersByMode()
    {
        var none = Array.Empty<BookedTime>().ToLookup(time => time.StaffUserId);

        var slots = GetConsultationSlotsQueryHandler.FreeSlots([Schedule()], none, MeetingMode.Phone, Monday.AddHours(-1), Monday.AddHours(4));

        var slot = Assert.Single(slots);
        Assert.Equal(Monday, slot.StartsOnUtc);
        Assert.Equal([MeetingMode.Phone], slot.Modes);
    }

    private static List<WeeklyWindow> Windows() =>
    [
        .. ScheduleSamples.NewYork().Windows!.Select(window =>
            new WeeklyWindow(window.Day, window.StartTime, window.EndTime, window.Mode))
    ];

    private static StaffScheduleResponse Schedule() =>
        new()
        {
            StaffUserId = StaffUserId,
            DisplayName = "Ms Rivera",
            TimeZone = "America/New_York",
            Windows =
            [
                .. ScheduleSamples.NewYork().Windows!.Select(window => new AvailabilityWindowResponse
                {
                    Day = window.Day,
                    StartTime = window.StartTime,
                    EndTime = window.EndTime,
                    Mode = window.Mode
                })
            ]
        };
}
