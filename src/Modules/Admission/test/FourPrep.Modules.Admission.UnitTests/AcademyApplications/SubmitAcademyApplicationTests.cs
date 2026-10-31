namespace FourPrep.Modules.Admission.UnitTests.AcademyApplications;

using FourPrep.Common.Domain.Enums;
using FourPrep.Modules.Admission.Domain.AcademyApplications;
using FourPrep.Modules.Admission.Application.AcademyApplications.SubmitAcademyApplication;

public class SubmitAcademyApplicationTests
{
    private static readonly Guid StudentUserId = Guid.NewGuid();
    private static readonly DateOnly AdultBirthday = new(2000, 5, 1);
    private static readonly DateOnly MinorBirthday = new(2010, 5, 1);

    [Fact]
    public async Task Handle_SubmitsTheApplicationForTheCurrentStudent()
    {
        var (handler, applications, unitOfWork) = Build(AdultBirthday);

        var result = await handler.Handle(Command() with { ParentConsentConfirmed = false }, default);

        Assert.True(result.IsSuccess);
        var application = Assert.Single(applications.Applications);
        Assert.Equal(result.Data, application.Id);
        Assert.Equal(StudentUserId, application.StudentUserId);
        Assert.Equal("Aziza Karimova", application.StudentName);
        Assert.Equal(AcademyApplicationStatus.Submitted, application.Status);
        Assert.Equal(new FakeClock().UtcNow, application.SubmittedOnUtc);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_NormalizesTheContactDetails()
    {
        var (handler, applications, _) = Build(AdultBirthday);

        await handler.Handle(
            Command() with
            {
                ContactChannels = [ContactChannel.Phone, ContactChannel.Telegram],
                PhoneNumber = " +1 415 555 0123 ",
                TelegramUsername = " @aziza_k ",
                MeetingLanguage = " UZ ",
                Question = "  How long does it take?  "
            },
            default);

        var application = Assert.Single(applications.Applications);
        Assert.Equal("+14155550123", application.PhoneNumber);
        Assert.Equal("aziza_k", application.TelegramUsername);
        Assert.Equal("uz", application.MeetingLanguage);
        Assert.Equal("How long does it take?", application.Question);
    }

    [Fact]
    public async Task Handle_DropsContactDetailsForChannelsNotChosen()
    {
        var (handler, applications, _) = Build(AdultBirthday);

        await handler.Handle(
            Command() with { PhoneNumber = "+14155550123", TelegramUsername = "aziza_k", Question = "  " },
            default);

        var application = Assert.Single(applications.Applications);
        Assert.Null(application.PhoneNumber);
        Assert.Null(application.TelegramUsername);
        Assert.Null(application.Question);
    }

    [Fact]
    public async Task Handle_KeepsTheReportedScores()
    {
        var (handler, applications, _) = Build(AdultBirthday);

        await handler.Handle(
            Command() with { Scores = [new ReportedScoreInput(Exam.Sat, 1350), new ReportedScoreInput(Exam.Ielts, 6.5m)] },
            default);

        var application = Assert.Single(applications.Applications);
        Assert.Equal(
            [(Exam.Sat, 1350m), (Exam.Ielts, 6.5m)],
            application.Scores.Select(score => (score.Exam, score.Score)));
    }

    [Fact]
    public async Task Handle_RefusesAMinorWithoutParentConsent()
    {
        var (handler, applications, unitOfWork) = Build(MinorBirthday);

        var result = await handler.Handle(Command() with { ParentConsentConfirmed = false }, default);

        Assert.Equal(AcademyApplicationErrors.ParentConsentRequired, result.Error);
        Assert.Empty(applications.Applications);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_TreatsAnUnknownBirthdayAsAMinor()
    {
        var (handler, applications, unitOfWork) = Build(null);

        var result = await handler.Handle(Command() with { ParentConsentConfirmed = false }, default);

        Assert.Equal(AcademyApplicationErrors.ParentConsentRequired, result.Error);
        Assert.Empty(applications.Applications);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_TreatsAStudentWhoTurns18TodayAsAnAdult()
    {
        var (handler, applications, _) = Build(new DateOnly(2008, 10, 31));

        var result = await handler.Handle(Command() with { ParentConsentConfirmed = false }, default);

        Assert.True(result.IsSuccess);
        Assert.Single(applications.Applications);
    }

    [Fact]
    public async Task Handle_AcceptsAMinorWithParentConsent()
    {
        var (handler, applications, _) = Build(MinorBirthday);

        var result = await handler.Handle(Command(), default);

        Assert.True(result.IsSuccess);
        Assert.True(Assert.Single(applications.Applications).ParentConsentConfirmed);
    }

    [Theory]
    [InlineData(AcademyApplicationStatus.Submitted)]
    [InlineData(AcademyApplicationStatus.Accepted)]
    public async Task Handle_RefusesASecondOpenApplication(AcademyApplicationStatus status)
    {
        var (handler, applications, unitOfWork) = Build(AdultBirthday);
        var existing = applications.Add(StudentUserId);
        if (status == AcademyApplicationStatus.Accepted)
        {
            existing.Accept(Guid.NewGuid(), new FakeClock().UtcNow);
        }

        var result = await handler.Handle(Command(), default);

        Assert.Equal(AcademyApplicationErrors.AlreadyApplied, result.Error);
        Assert.Single(applications.Applications);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_LetsAStudentApplyAgainAfterWithdrawing()
    {
        var (handler, applications, _) = Build(AdultBirthday);
        applications.Add(StudentUserId).Withdraw(new FakeClock().UtcNow);

        var result = await handler.Handle(Command(), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, applications.Applications.Count);
    }

    [Fact]
    public async Task Handle_FailsWhenTheStudentProfileIsMissing()
    {
        var applications = new FakeAcademyApplicationRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = Handler(new FakeStudentService(), applications, unitOfWork);

        var result = await handler.Handle(Command(), default);

        Assert.Equal(FakeStudentService.NotFound, result.Error);
        Assert.Empty(applications.Applications);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    internal static SubmitAcademyApplicationCommand Command() =>
        new(
            [ApplicationInterest.AdmissionPackage, ApplicationInterest.TestPrep],
            [new ReportedScoreInput(Exam.Ielts, 6.5m)],
            [ContactChannel.Email],
            null,
            null,
            "en",
            "Which countries fit my budget?",
            true);

    private static (SubmitAcademyApplicationCommandHandler Handler, FakeAcademyApplicationRepository Applications, FakeUnitOfWork UnitOfWork) Build(
        DateOnly? dateOfBirth)
    {
        var students = new FakeStudentService();
        students.AddStudent(StudentUserId, dateOfBirth);
        var applications = new FakeAcademyApplicationRepository();
        var unitOfWork = new FakeUnitOfWork();

        return (Handler(students, applications, unitOfWork), applications, unitOfWork);
    }

    private static SubmitAcademyApplicationCommandHandler Handler(
        FakeStudentService students,
        FakeAcademyApplicationRepository applications,
        FakeUnitOfWork unitOfWork) =>
        new(
            unitOfWork,
            new FakeCurrentUser(StudentUserId),
            students,
            applications,
            new FakeTimeZoneContext(),
            new FakeClock());
}
