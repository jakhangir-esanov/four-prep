namespace FourPrep.Modules.Meeting.UnitTests.Meetings;

using FourPrep.Modules.Meeting.Domain.Meetings;
using FourPrep.Modules.Meeting.Domain.StaffSchedules;
using FourPrep.Modules.Meeting.Application.Meetings;
using FourPrep.Modules.Meeting.Application.Meetings.GetMeetings;
using FourPrep.Modules.Meeting.Application.Meetings.GetConsultationSlots;
using FourPrep.Modules.Meeting.Application.Meetings.CancelMeeting;
using FourPrep.Modules.Meeting.Application.Meetings.RecordAttendance;
using FourPrep.Modules.Meeting.Application.Meetings.ChangeMeetingLink;
using FourPrep.Modules.Meeting.Application.Meetings.StaffCancelMeeting;
using FourPrep.Modules.Meeting.Application.Meetings.UpdateMeetingNotes;

public class MeetingCommandTests
{
    private static readonly Guid ParentId = Guid.NewGuid();
    private static readonly Guid StudentUserId = Guid.NewGuid();
    private static readonly Guid StaffUserId = Guid.NewGuid();

    private static readonly DateTime Monday = ScheduleSamples.MondayNineInNewYorkUtc;

    [Fact]
    public async Task Cancel_LetsTheFamilyCancelBeforeTheStart()
    {
        var (meetings, students, unitOfWork) = Build();
        var meeting = meetings.Add(StudentUserId, StaffUserId, Monday);

        var result = await FamilyCancel(meetings, students, unitOfWork, ParentId).Handle(new CancelMeetingCommand(meeting.Id), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(MeetingStatus.Cancelled, meeting.Status);
        Assert.Equal(ParentId, meeting.CancelledByUserId);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Cancel_HidesAnotherFamilysMeeting()
    {
        var (meetings, students, unitOfWork) = Build();
        var meeting = meetings.Add(StudentUserId, StaffUserId, Monday);

        var result = await FamilyCancel(meetings, students, unitOfWork, Guid.NewGuid()).Handle(new CancelMeetingCommand(meeting.Id), default);

        Assert.Equal(MeetingErrors.NotFound, result.Error);
        Assert.Equal(MeetingStatus.Scheduled, meeting.Status);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Cancel_RefusesAMeetingThatHasStarted()
    {
        var (meetings, students, unitOfWork) = Build();
        var meeting = meetings.Add(StudentUserId, StaffUserId, new FakeClock().UtcNow.AddMinutes(-10));

        var result = await FamilyCancel(meetings, students, unitOfWork, ParentId).Handle(new CancelMeetingCommand(meeting.Id), default);

        Assert.Equal(MeetingErrors.AlreadyStarted, result.Error);
        Assert.Equal(MeetingStatus.Scheduled, meeting.Status);
    }

    [Fact]
    public async Task Cancel_RefusesAMeetingThatIsNoLongerScheduled()
    {
        var (meetings, students, unitOfWork) = Build();
        var meeting = meetings.Add(StudentUserId, StaffUserId, Monday);
        meeting.Cancel(StaffUserId, "Sick", Monday.AddDays(-2));

        var result = await FamilyCancel(meetings, students, unitOfWork, ParentId).Handle(new CancelMeetingCommand(meeting.Id), default);

        Assert.Equal(MeetingErrors.NotScheduled, result.Error);
        Assert.Equal(StaffUserId, meeting.CancelledByUserId);
    }

    [Fact]
    public async Task Cancel_ForgetsTheStudentPhone()
    {
        var (meetings, students, unitOfWork) = Build();
        var meeting = meetings.Add(StudentUserId, StaffUserId, Monday, MeetingMode.Phone);

        await FamilyCancel(meetings, students, unitOfWork, ParentId).Handle(new CancelMeetingCommand(meeting.Id), default);

        Assert.Null(meeting.StudentPhone);
    }

    [Fact]
    public async Task StaffCancel_KeepsTheReason()
    {
        var (meetings, _, unitOfWork) = Build();
        var meeting = meetings.Add(StudentUserId, StaffUserId, Monday);

        var result = await StaffCancel(meetings, unitOfWork, StaffUserId).Handle(new StaffCancelMeetingCommand(meeting.Id, "  I am ill  "), default);

        Assert.True(result.IsSuccess);
        Assert.Equal("I am ill", meeting.CancellationReason);
        Assert.Equal(StaffUserId, meeting.CancelledByUserId);
    }

    [Fact]
    public async Task StaffCancel_HidesAnotherStaffMembersMeeting()
    {
        var (meetings, _, unitOfWork) = Build();
        var meeting = meetings.Add(StudentUserId, StaffUserId, Monday);

        var result = await StaffCancel(meetings, unitOfWork, Guid.NewGuid()).Handle(new StaffCancelMeetingCommand(meeting.Id, null), default);

        Assert.Equal(MeetingErrors.NotFound, result.Error);
        Assert.Equal(MeetingStatus.Scheduled, meeting.Status);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task StaffCancel_LetsAnAdminCancelAnyMeeting()
    {
        var (meetings, _, unitOfWork) = Build();
        var adminId = Guid.NewGuid();
        var meeting = meetings.Add(StudentUserId, StaffUserId, Monday);

        var result = await StaffCancel(meetings, unitOfWork, adminId).Handle(new StaffCancelMeetingCommand(meeting.Id, null, AnyStaff: true), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(adminId, meeting.CancelledByUserId);
    }

    [Fact]
    public async Task Attendance_IsRecordedOnlyAfterTheStart()
    {
        var (meetings, _, unitOfWork) = Build();
        var meeting = meetings.Add(StudentUserId, StaffUserId, Monday);

        var result = await Attendance(meetings, unitOfWork, StaffUserId).Handle(new RecordAttendanceCommand(meeting.Id, MeetingStatus.Held), default);

        Assert.Equal(MeetingErrors.NotStartedYet, result.Error);
        Assert.Equal(MeetingStatus.Scheduled, meeting.Status);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Attendance_MarksAStartedMeetingAndCanBeCorrected()
    {
        var (meetings, _, unitOfWork) = Build();
        var meeting = meetings.Add(StudentUserId, StaffUserId, new FakeClock().UtcNow.AddHours(-1));

        await Attendance(meetings, unitOfWork, StaffUserId).Handle(new RecordAttendanceCommand(meeting.Id, MeetingStatus.NoShow), default);
        var result = await Attendance(meetings, unitOfWork, StaffUserId).Handle(new RecordAttendanceCommand(meeting.Id, MeetingStatus.Held), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(MeetingStatus.Held, meeting.Status);
        Assert.Equal(2, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Attendance_RefusesACancelledMeeting()
    {
        var (meetings, _, unitOfWork) = Build();
        var meeting = meetings.Add(StudentUserId, StaffUserId, new FakeClock().UtcNow.AddHours(-1));
        meeting.Cancel(ParentId, null, new FakeClock().UtcNow.AddDays(-1));

        var result = await Attendance(meetings, unitOfWork, StaffUserId).Handle(new RecordAttendanceCommand(meeting.Id, MeetingStatus.Held), default);

        Assert.Equal(MeetingErrors.NotScheduled, result.Error);
    }

    [Fact]
    public async Task Attendance_HidesAnotherStaffMembersMeeting()
    {
        var (meetings, _, unitOfWork) = Build();
        var meeting = meetings.Add(StudentUserId, StaffUserId, new FakeClock().UtcNow.AddHours(-1));

        var result = await Attendance(meetings, unitOfWork, Guid.NewGuid()).Handle(new RecordAttendanceCommand(meeting.Id, MeetingStatus.Held), default);

        Assert.Equal(MeetingErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Attendance_ForgetsTheStudentPhone()
    {
        var (meetings, _, unitOfWork) = Build();
        var meeting = meetings.Add(StudentUserId, StaffUserId, new FakeClock().UtcNow.AddHours(-1), MeetingMode.Phone);

        await Attendance(meetings, unitOfWork, StaffUserId).Handle(new RecordAttendanceCommand(meeting.Id, MeetingStatus.Held), default);

        Assert.Null(meeting.StudentPhone);
    }

    [Fact]
    public void AttendanceValidator_AcceptsOnlyHeldOrNoShow()
    {
        var error = Assert.Single(new RecordAttendanceCommandValidator()
            .Validate(new RecordAttendanceCommand(Guid.NewGuid(), MeetingStatus.Cancelled)).Errors);

        Assert.Equal(MeetingErrors.InvalidAttendance.Code, error.ErrorCode);
    }

    [Fact]
    public async Task Notes_AreKeptForTheStaffMember()
    {
        var (meetings, _, unitOfWork) = Build();
        var meeting = meetings.Add(StudentUserId, StaffUserId, Monday);

        var result = await new UpdateMeetingNotesCommandHandler(unitOfWork, new FakeCurrentUser(StaffUserId), meetings, new FakeClock())
            .Handle(new UpdateMeetingNotesCommand(meeting.Id, " Wants to study CS in Canada "), default);

        Assert.True(result.IsSuccess);
        Assert.Equal("Wants to study CS in Canada", meeting.StaffNotes);
    }

    [Fact]
    public async Task Notes_HideAnotherStaffMembersMeeting()
    {
        var (meetings, _, unitOfWork) = Build();
        var meeting = meetings.Add(StudentUserId, StaffUserId, Monday);

        var result = await new UpdateMeetingNotesCommandHandler(unitOfWork, new FakeCurrentUser(Guid.NewGuid()), meetings, new FakeClock())
            .Handle(new UpdateMeetingNotesCommand(meeting.Id, "Peeking"), default);

        Assert.Equal(MeetingErrors.NotFound, result.Error);
        Assert.Null(meeting.StaffNotes);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public void NotesValidator_LimitsTheLength()
    {
        var errors = new UpdateMeetingNotesCommandValidator()
            .Validate(new UpdateMeetingNotesCommand(Guid.NewGuid(), new string('a', 4001))).Errors;

        Assert.Contains(errors, error => error.PropertyName == nameof(UpdateMeetingNotesCommand.Notes));
    }

    [Fact]
    public void StaffCancelValidator_LimitsTheReason()
    {
        var errors = new StaffCancelMeetingCommandValidator()
            .Validate(new StaffCancelMeetingCommand(Guid.NewGuid(), new string('a', 501))).Errors;

        Assert.Contains(errors, error => error.PropertyName == nameof(StaffCancelMeetingCommand.Reason));
    }

    [Fact]
    public async Task Link_CanBeChangedForOneOnlineMeeting()
    {
        var (meetings, _, unitOfWork) = Build();
        var meeting = meetings.Add(StudentUserId, StaffUserId, Monday);

        var result = await Link(meetings, unitOfWork).Handle(new ChangeMeetingLinkCommand(meeting.Id, "https://meet.google.com/abc-defg-hij"), default);

        Assert.True(result.IsSuccess);
        Assert.Equal("https://meet.google.com/abc-defg-hij", meeting.MeetingUrl);
    }

    [Fact]
    public async Task Link_IsRefusedForAPhoneMeeting()
    {
        var (meetings, _, unitOfWork) = Build();
        var meeting = meetings.Add(StudentUserId, StaffUserId, Monday, MeetingMode.Phone);

        var result = await Link(meetings, unitOfWork).Handle(new ChangeMeetingLinkCommand(meeting.Id, "https://meet.google.com/abc-defg-hij"), default);

        Assert.Equal(MeetingErrors.NotOnline, result.Error);
        Assert.Null(meeting.MeetingUrl);
    }

    [Fact]
    public async Task Link_HidesAnotherStaffMembersMeeting()
    {
        var (meetings, _, unitOfWork) = Build();
        var meeting = meetings.Add(StudentUserId, StaffUserId, Monday);

        var result = await new ChangeMeetingLinkCommandHandler(unitOfWork, new FakeCurrentUser(Guid.NewGuid()), meetings, new FakeClock())
            .Handle(new ChangeMeetingLinkCommand(meeting.Id, "https://meet.google.com/abc-defg-hij"), default);

        Assert.Equal(MeetingErrors.NotFound, result.Error);
        Assert.Equal("https://zoom.us/j/1234567890", meeting.MeetingUrl);
    }

    [Theory]
    [InlineData("http://zoom.us/j/1")]
    [InlineData("https://zoom.us@evil.example/j/1")]
    [InlineData("https://zoom.us.evil.example/j/1")]
    [InlineData("https://evilzoom.us/j/1")]
    [InlineData("https://127.0.0.1/j/1")]
    [InlineData("https://localhost/j/1")]
    [InlineData("https://zoom.us:8443/j/1")]
    [InlineData("https://teams.microsoft.com/l/meetup-join/1")]
    public void LinkValidator_RefusesAnythingButAZoomOrGoogleMeetAddress(string url)
    {
        var error = Assert.Single(new ChangeMeetingLinkCommandValidator()
            .Validate(new ChangeMeetingLinkCommand(Guid.NewGuid(), url)).Errors);

        Assert.Equal(StaffScheduleErrors.InvalidMeetingUrl.Code, error.ErrorCode);
    }

    [Theory]
    [InlineData("https://zoom.us/j/1234567890?pwd=abc")]
    [InlineData("https://us02web.zoom.us/j/1234567890")]
    [InlineData("https://meet.google.com/abc-defg-hij")]
    public void MeetingLinks_AcceptZoomAndGoogleMeet(string url)
    {
        Assert.True(MeetingLinks.IsValid(url));
    }

    [Fact]
    public async Task StudentMeetings_RefuseSomebodyOutsideTheFamily()
    {
        var (_, students, _) = Build();

        var result = await new GetStudentMeetingsQueryHandler(new FakeCurrentUser(Guid.NewGuid()), students, new UnusedConnectionFactory())
            .Handle(new GetStudentMeetingsQuery(StudentUserId), default);

        Assert.Equal(MeetingErrors.NotYourStudent, result.Error);
    }

    [Fact]
    public async Task Slots_RefuseSomebodyOutsideTheFamily()
    {
        var (_, students, _) = Build();

        var result = await new GetConsultationSlotsQueryHandler(
                new FakeCurrentUser(Guid.NewGuid()),
                students,
                new FakeClock(),
                new UnusedConnectionFactory())
            .Handle(new GetConsultationSlotsQuery(StudentUserId), default);

        Assert.Equal(MeetingErrors.NotYourStudent, result.Error);
    }

    [Fact]
    public void SlotsValidator_RefusesAnEndBeforeTheStartOrMoreThanAMonth()
    {
        var validator = new GetConsultationSlotsQueryValidator();
        var start = Monday;

        Assert.Equal(
            MeetingErrors.InvalidRange.Code,
            Assert.Single(validator.Validate(new GetConsultationSlotsQuery(StudentUserId, null, start, start.AddHours(-1))).Errors).ErrorCode);
        Assert.Equal(
            MeetingErrors.InvalidRange.Code,
            Assert.Single(validator.Validate(new GetConsultationSlotsQuery(StudentUserId, null, start, start.AddDays(32))).Errors).ErrorCode);
        Assert.True(validator.Validate(new GetConsultationSlotsQuery(StudentUserId, null, start, start.AddDays(31))).IsValid);
    }

    [Fact]
    public void UtcTimes_TreatsATimeWithoutAZoneAsUtc()
    {
        var unspecified = new DateTime(2026, 11, 2, 14, 0, 0, DateTimeKind.Unspecified);

        Assert.Equal(Monday, UtcTimes.From(unspecified));
        Assert.Equal(DateTimeKind.Utc, UtcTimes.From(unspecified).Kind);
        Assert.Equal(Monday, UtcTimes.From(Monday.ToLocalTime()));
    }

    [Fact]
    public void ForFamily_HidesStaffNotesAndTheLinkOfAMeetingThatIsOver()
    {
        var response = new MeetingResponse
        {
            Status = MeetingStatus.Cancelled,
            StaffUserId = StaffUserId,
            BookedByUserId = ParentId,
            StaffNotes = "Private",
            MeetingUrl = "https://zoom.us/j/1"
        };

        var forFamily = response.ForFamily(ParentId);

        Assert.Null(forFamily.StaffNotes);
        Assert.Null(forFamily.MeetingUrl);
        Assert.Null(forFamily.StaffUserId);
        Assert.Null(forFamily.BookedByUserId);
        Assert.True(forFamily.BookedByMe);
    }

    [Fact]
    public void ForFamily_KeepsTheLinkOfAScheduledMeeting()
    {
        var response = new MeetingResponse { Status = MeetingStatus.Scheduled, MeetingUrl = "https://zoom.us/j/1" };

        Assert.Equal("https://zoom.us/j/1", response.ForFamily(ParentId).MeetingUrl);
    }

    private static (FakeMeetingRepository Meetings, FakeStudentService Students, FakeUnitOfWork UnitOfWork) Build()
    {
        var students = new FakeStudentService();
        students.AddFamily(ParentId, StudentUserId);
        return (new FakeMeetingRepository(), students, new FakeUnitOfWork());
    }

    private static CancelMeetingCommandHandler FamilyCancel(
        FakeMeetingRepository meetings,
        FakeStudentService students,
        FakeUnitOfWork unitOfWork,
        Guid userId) =>
        new(unitOfWork, new FakeCurrentUser(userId), students, meetings, new FakeClock());

    private static StaffCancelMeetingCommandHandler StaffCancel(FakeMeetingRepository meetings, FakeUnitOfWork unitOfWork, Guid userId) =>
        new(unitOfWork, new FakeCurrentUser(userId), meetings, new FakeClock());

    private static RecordAttendanceCommandHandler Attendance(FakeMeetingRepository meetings, FakeUnitOfWork unitOfWork, Guid userId) =>
        new(unitOfWork, new FakeCurrentUser(userId), meetings, new FakeClock());

    private static ChangeMeetingLinkCommandHandler Link(FakeMeetingRepository meetings, FakeUnitOfWork unitOfWork) =>
        new(unitOfWork, new FakeCurrentUser(StaffUserId), meetings, new FakeClock());
}
