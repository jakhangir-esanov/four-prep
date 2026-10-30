namespace FourPrep.Modules.Meeting.UnitTests.StaffSchedules;

using FourPrep.Common.Domain.Localization;
using FourPrep.Modules.Meeting.Domain.Meetings;
using FourPrep.Modules.Meeting.Domain.StaffSchedules;
using FourPrep.Modules.Meeting.Application.StaffSchedules;
using FourPrep.Modules.Meeting.Domain.AvailabilityWindows;
using FourPrep.Modules.Meeting.Application.StaffSchedules.SaveMySchedule;

public class StaffScheduleTests
{
    private static readonly Guid StaffUserId = Guid.NewGuid();

    private static readonly SaveMyScheduleCommandValidator Validator = new();

    [Fact]
    public void Validate_WhenTheScheduleIsComplete_Passes()
    {
        Assert.True(Validator.Validate(new SaveMyScheduleCommand(ScheduleSamples.NewYork())).IsValid);
    }

    [Fact]
    public void Validate_WhenThereAreNoWindowsYet_Passes()
    {
        var details = ScheduleSamples.NewYork() with { Windows = [], MeetingUrl = null };

        Assert.True(Validator.Validate(new SaveMyScheduleCommand(details)).IsValid);
    }

    [Fact]
    public void Validate_WhenOnlineIsOfferedWithoutALink_FailsWithMeetingUrlRequired()
    {
        AssertSingleError(ScheduleSamples.NewYork() with { MeetingUrl = " " }, StaffScheduleErrors.MeetingUrlRequired.Code, "Details.MeetingUrl");
    }

    [Theory]
    [InlineData("http://zoom.us/j/1234567890")]
    [InlineData("zoom.us/j/1234567890")]
    [InlineData("javascript:alert(1)")]
    public void Validate_WhenTheLinkIsNotHttps_FailsWithInvalidMeetingUrl(string url)
    {
        AssertSingleError(ScheduleSamples.NewYork() with { MeetingUrl = url }, StaffScheduleErrors.InvalidMeetingUrl.Code, "Details.MeetingUrl");
    }

    [Fact]
    public void Validate_WhenTheOfficeIsOfferedWithoutAnAddress_FailsWithOfficeRequired()
    {
        AssertSingleError(ScheduleSamples.NewYork() with { OfficeAddress = null }, StaffScheduleErrors.OfficeRequired.Code, "Details.OfficeAddress");
    }

    [Fact]
    public void Validate_WhenTheTimeZoneIsUnknown_FailsWithInvalidTimeZone()
    {
        AssertSingleError(ScheduleSamples.NewYork() with { TimeZone = "Mars/Olympus" }, LocalizationErrors.InvalidTimeZone.Code, "Details.TimeZone");
    }

    [Fact]
    public void Validate_WhenAWindowCannotHoldAConsultation_FailsWithTooShort()
    {
        var details = ScheduleSamples.NewYork() with
        {
            Windows = [new AvailabilityWindowInput(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(9, 30), MeetingMode.Online)]
        };

        AssertSingleError(details, AvailabilityWindowErrors.TooShort.Code, "Details.Windows[0].EndTime");
    }

    [Fact]
    public void Validate_WhenTwoWindowsOfOneModeOverlap_FailsWithOverlapping()
    {
        var details = ScheduleSamples.NewYork() with
        {
            Windows =
            [
                new AvailabilityWindowInput(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(11, 0), MeetingMode.Online),
                new AvailabilityWindowInput(DayOfWeek.Monday, new TimeOnly(10, 30), new TimeOnly(12, 0), MeetingMode.Online)
            ]
        };

        AssertSingleError(details, AvailabilityWindowErrors.Overlapping.Code, "Details.Windows");
    }

    [Fact]
    public async Task Save_CreatesTheScheduleWithNormalizedValues()
    {
        var schedules = new FakeStaffScheduleRepository();
        var unitOfWork = new FakeUnitOfWork();

        var result = await Handler(schedules, unitOfWork).Handle(new SaveMyScheduleCommand(ScheduleSamples.NewYork()), default);

        Assert.True(result.IsSuccess);
        var schedule = Assert.Single(schedules.Schedules);
        Assert.Equal(StaffUserId, schedule.StaffUserId);
        Assert.Equal("Ms Rivera", schedule.DisplayName);
        Assert.Equal("https://zoom.us/j/1234567890", schedule.MeetingUrl);
        Assert.Equal("US", schedule.OfficeCountryCode);
        Assert.Equal(3, schedule.Windows.Count);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Save_ReplacesTheWindowsOfAnExistingSchedule()
    {
        var schedules = new FakeStaffScheduleRepository();
        var unitOfWork = new FakeUnitOfWork();
        var schedule = schedules.Add(StaffUserId);
        var details = ScheduleSamples.NewYork() with
        {
            Windows = [new AvailabilityWindowInput(DayOfWeek.Friday, new TimeOnly(15, 0), new TimeOnly(17, 0), MeetingMode.Online)]
        };

        var result = await Handler(schedules, unitOfWork).Handle(new SaveMyScheduleCommand(details), default);

        Assert.True(result.IsSuccess);
        Assert.Single(schedules.Schedules);
        var window = Assert.Single(schedule.Windows);
        Assert.Equal(DayOfWeek.Friday, window.Day);
        Assert.Equal(new FakeClock().UtcNow, schedule.UpdatedOnUtc);
    }

    private static SaveMyScheduleCommandHandler Handler(FakeStaffScheduleRepository schedules, FakeUnitOfWork unitOfWork) =>
        new(unitOfWork, new FakeCurrentUser(StaffUserId), schedules, new FakeClock());

    private static void AssertSingleError(StaffScheduleDetails details, string code, string propertyName)
    {
        var error = Assert.Single(Validator.Validate(new SaveMyScheduleCommand(details)).Errors);

        Assert.Equal(code, error.ErrorCode);
        Assert.Equal(propertyName, error.PropertyName);
    }
}
