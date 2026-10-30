namespace FourPrep.Modules.Meeting.UnitTests.Meetings;

using FourPrep.Modules.Meeting.Domain.Meetings;
using FourPrep.Modules.Meeting.Application.Meetings.BookConsultation;

public class BookConsultationTests
{
    private static readonly Guid ParentId = Guid.NewGuid();
    private static readonly Guid StudentUserId = Guid.NewGuid();
    private static readonly Guid StaffUserId = Guid.NewGuid();

    private static readonly DateTime Monday = ScheduleSamples.MondayNineInNewYorkUtc;

    [Fact]
    public async Task Book_RecordsAnOnlineConsultationWithTheStaffLink()
    {
        var fixture = new Fixture();

        var result = await fixture.Book(Command());

        Assert.True(result.IsSuccess);
        var meeting = Assert.Single(fixture.Meetings.Meetings);
        Assert.Equal(result.Data, meeting.Id);
        Assert.Equal(MeetingKind.Consultation, meeting.Kind);
        Assert.Equal(MeetingStatus.Scheduled, meeting.Status);
        Assert.Equal(Monday, meeting.StartsOnUtc);
        Assert.Equal(Meeting.ConsultationMinutes, meeting.DurationMinutes);
        Assert.Equal("https://zoom.us/j/1234567890", meeting.MeetingUrl);
        Assert.Equal("Ms Rivera", meeting.StaffName);
        Assert.Equal("Aziza Karimova", meeting.StudentName);
        Assert.Equal(ParentId, meeting.BookedByUserId);
        Assert.Null(meeting.StudentPhone);
        Assert.Null(meeting.OfficeAddress);
        Assert.Equal(new FakeClock().UtcNow, fixture.Schedule.LastBookedOnUtc);
        Assert.Equal(1, fixture.UnitOfWork.SaveCount);
    }

    [Fact]
    public async Task Book_KeepsTheNormalizedPhoneOfAPhoneConsultation()
    {
        var fixture = new Fixture();

        var result = await fixture.Book(Command() with { Mode = MeetingMode.Phone, PhoneNumber = "+1 415 555 0123" });

        Assert.True(result.IsSuccess);
        var meeting = Assert.Single(fixture.Meetings.Meetings);
        Assert.Equal("+14155550123", meeting.StudentPhone);
        Assert.Null(meeting.MeetingUrl);
    }

    [Fact]
    public async Task Book_KeepsTheOfficeAddressOfAnOfficeConsultation()
    {
        var fixture = new Fixture();
        var tuesdayTwoPm = new DateTime(2026, 11, 3, 19, 0, 0, DateTimeKind.Utc);

        var result = await fixture.Book(Command() with { Mode = MeetingMode.Office, StartsOnUtc = tuesdayTwoPm });

        Assert.True(result.IsSuccess);
        var meeting = Assert.Single(fixture.Meetings.Meetings);
        Assert.Equal("US", meeting.OfficeCountryCode);
        Assert.Equal("New York", meeting.OfficeCity);
        Assert.Equal("1 Example Avenue", meeting.OfficeAddress);
        Assert.Null(meeting.MeetingUrl);
    }

    [Fact]
    public async Task Book_LetsTheStudentBookWithTheirOwnMentor()
    {
        var fixture = new Fixture(mentorUserId: StaffUserId);

        var result = await fixture.Book(Command());

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Book_RefusesSomebodyOutsideTheFamily()
    {
        var fixture = new Fixture();

        var result = await fixture.Book(Command(), userId: Guid.NewGuid());

        Assert.Equal(MeetingErrors.NotYourStudent, result.Error);
        fixture.AssertNothingBooked();
    }

    [Fact]
    public async Task Book_RefusesAnotherStaffWhenTheStudentHasAMentor()
    {
        var fixture = new Fixture(mentorUserId: Guid.NewGuid());

        var result = await fixture.Book(Command());

        Assert.Equal(MeetingErrors.MentorOnly, result.Error);
        fixture.AssertNothingBooked();
    }

    [Fact]
    public async Task Book_RefusesStaffWithoutASchedule()
    {
        var fixture = new Fixture();

        var result = await fixture.Book(Command() with { StaffUserId = Guid.NewGuid() });

        Assert.Equal(MeetingErrors.SlotNotAvailable, result.Error);
        fixture.AssertNothingBooked();
    }

    [Theory]
    [InlineData(15, MeetingMode.Online)]
    [InlineData(90, MeetingMode.Online)]
    [InlineData(30, MeetingMode.Phone)]
    [InlineData(0, MeetingMode.Office)]
    public async Task Book_RefusesATimeTheScheduleDoesNotOffer(int minutesAfterNine, MeetingMode mode)
    {
        var fixture = new Fixture();

        var result = await fixture.Book(Command() with { StartsOnUtc = Monday.AddMinutes(minutesAfterNine), Mode = mode, PhoneNumber = "+14155550123" });

        Assert.Equal(MeetingErrors.SlotNotAvailable, result.Error);
        fixture.AssertNothingBooked();
    }

    [Fact]
    public async Task Book_RefusesATimeLessThanTwelveHoursAway()
    {
        var fixture = new Fixture();
        fixture.Clock.UtcNow = Monday.AddHours(-11);

        var result = await fixture.Book(Command());

        Assert.Equal(MeetingErrors.SlotNotAvailable, result.Error);
        fixture.AssertNothingBooked();
    }

    [Fact]
    public async Task Book_RefusesATimeThatOverlapsAnotherMeetingOfTheStaff()
    {
        var fixture = new Fixture();
        fixture.Meetings.Add(Guid.NewGuid(), StaffUserId, Monday.AddMinutes(-30));

        var result = await fixture.Book(Command());

        Assert.Equal(MeetingErrors.SlotTaken, result.Error);
        Assert.Single(fixture.Meetings.Meetings);
        Assert.Equal(0, fixture.UnitOfWork.SaveCount);
    }

    [Fact]
    public async Task Book_AllowsOneScheduledConsultationPerStudent()
    {
        var fixture = new Fixture();
        fixture.Meetings.Add(StudentUserId, Guid.NewGuid(), Monday.AddDays(1));

        var result = await fixture.Book(Command());

        Assert.Equal(MeetingErrors.AlreadyBooked, result.Error);
        Assert.Single(fixture.Meetings.Meetings);
    }

    [Fact]
    public async Task Book_AllowsANewConsultationAfterTheLastOneWasCancelled()
    {
        var fixture = new Fixture();
        fixture.Meetings.Add(StudentUserId, Guid.NewGuid(), Monday.AddDays(1)).Cancel(ParentId, null, Monday.AddDays(-5));

        var result = await fixture.Book(Command());

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Book_RefusesAScheduleWhoseOwnerIsNoLongerAConsultant()
    {
        var fixture = new Fixture();
        fixture.Auth.Roles[StaffUserId] = ["teacher"];

        var result = await fixture.Book(Command());

        Assert.Equal(MeetingErrors.SlotNotAvailable, result.Error);
        fixture.AssertNothingBooked();
    }

    [Fact]
    public async Task Book_LetsOneUserHoldAtMostThreeScheduledConsultations()
    {
        var fixture = new Fixture();
        for (var i = 0; i < Meeting.MaxScheduledPerBooker; i++)
        {
            fixture.Meetings.Add(Guid.NewGuid(), Guid.NewGuid(), Monday.AddDays(i + 1), bookedByUserId: ParentId);
        }

        var result = await fixture.Book(Command());

        Assert.Equal(MeetingErrors.TooManyBooked, result.Error);
        Assert.Equal(Meeting.MaxScheduledPerBooker, fixture.Meetings.Meetings.Count);
        Assert.Equal(0, fixture.UnitOfWork.SaveCount);
    }

    [Fact]
    public async Task Book_ReadsAnOffsetTimeAsTheSameInstant()
    {
        var fixture = new Fixture();
        var sameInstantAsLocal = Monday.ToLocalTime();

        var result = await fixture.Book(Command() with { StartsOnUtc = sameInstantAsLocal });

        Assert.True(result.IsSuccess);
        Assert.Equal(Monday, Assert.Single(fixture.Meetings.Meetings).StartsOnUtc);
    }

    [Fact]
    public void Validate_WhenAPhoneConsultationHasNoPhone_FailsWithPhoneRequired()
    {
        var error = Assert.Single(new BookConsultationCommandValidator().Validate(Command() with { Mode = MeetingMode.Phone }).Errors);

        Assert.Equal(MeetingErrors.PhoneRequired.Code, error.ErrorCode);
        Assert.Equal(nameof(BookConsultationCommand.PhoneNumber), error.PropertyName);
    }

    [Fact]
    public void Validate_WhenThePhoneIsNotInternational_Fails()
    {
        var errors = new BookConsultationCommandValidator().Validate(Command() with { Mode = MeetingMode.Phone, PhoneNumber = "12345" }).Errors;

        Assert.Contains(errors, error => error.PropertyName == nameof(BookConsultationCommand.PhoneNumber));
    }

    private static BookConsultationCommand Command() =>
        new(StudentUserId, StaffUserId, Monday, MeetingMode.Online);

    private sealed class Fixture
    {
        public Fixture(Guid? mentorUserId = null)
        {
            Students.AddFamily(ParentId, StudentUserId, mentorUserId);
            Auth.AddMentor(StaffUserId);
            Schedule = Schedules.Add(StaffUserId);
        }

        public FakeClock Clock { get; } = new();
        public FakeUnitOfWork UnitOfWork { get; } = new();
        public FakeStudentService Students { get; } = new();
        public FakeAuthService Auth { get; } = new();
        public FakeMeetingRepository Meetings { get; } = new();
        public FakeStaffScheduleRepository Schedules { get; } = new();
        public Domain.StaffSchedules.StaffSchedule Schedule { get; }

        public Task<FourPrep.Common.Domain.Result<Guid>> Book(BookConsultationCommand command, Guid? userId = null) =>
            new BookConsultationCommandHandler(
                    UnitOfWork,
                    new FakeCurrentUser(userId ?? ParentId),
                    Auth,
                    Students,
                    Meetings,
                    Schedules,
                    Clock)
                .Handle(command, default);

        public void AssertNothingBooked()
        {
            Assert.Empty(Meetings.Meetings);
            Assert.Null(Schedule.LastBookedOnUtc);
            Assert.Equal(0, UnitOfWork.SaveCount);
        }
    }
}
