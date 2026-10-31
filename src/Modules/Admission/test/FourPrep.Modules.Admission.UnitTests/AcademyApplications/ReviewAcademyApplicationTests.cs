namespace FourPrep.Modules.Admission.UnitTests.AcademyApplications;

using FourPrep.Modules.Admission.Domain.AcademyApplications;
using FourPrep.Modules.Admission.Application.AcademyApplications.AcceptAcademyApplication;
using FourPrep.Modules.Admission.Application.AcademyApplications.DeclineAcademyApplication;
using FourPrep.Modules.Admission.Application.AcademyApplications.WithdrawAcademyApplication;

public class ReviewAcademyApplicationTests
{
    private static readonly Guid StudentUserId = Guid.NewGuid();
    private static readonly Guid AdminUserId = Guid.NewGuid();

    [Fact]
    public async Task Accept_RecordsWhoAcceptedAndWhen()
    {
        var (applications, unitOfWork) = Build();
        var application = applications.Add(StudentUserId);

        var result = await Accept(applications, unitOfWork).Handle(new AcceptAcademyApplicationCommand(application.Id), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(AcademyApplicationStatus.Accepted, application.Status);
        Assert.Equal(AdminUserId, application.ReviewedByUserId);
        Assert.Equal(new FakeClock().UtcNow, application.ReviewedOnUtc);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Accept_FailsForAnUnknownApplication()
    {
        var (applications, unitOfWork) = Build();

        var result = await Accept(applications, unitOfWork).Handle(new AcceptAcademyApplicationCommand(Guid.NewGuid()), default);

        Assert.Equal(AcademyApplicationErrors.NotFound, result.Error);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Accept_RefusesAWithdrawnApplication()
    {
        var (applications, unitOfWork) = Build();
        var application = applications.Add(StudentUserId);
        application.Withdraw(new FakeClock().UtcNow);

        var result = await Accept(applications, unitOfWork).Handle(new AcceptAcademyApplicationCommand(application.Id), default);

        Assert.Equal(AcademyApplicationErrors.NotSubmitted, result.Error);
        Assert.Equal(AcademyApplicationStatus.Withdrawn, application.Status);
        Assert.Null(application.ReviewedByUserId);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Decline_KeepsTheTrimmedReason()
    {
        var (applications, unitOfWork) = Build();
        var application = applications.Add(StudentUserId);

        var result = await Decline(applications, unitOfWork).Handle(
            new DeclineAcademyApplicationCommand(application.Id, "  We do not serve this destination yet.  "),
            default);

        Assert.True(result.IsSuccess);
        Assert.Equal(AcademyApplicationStatus.Declined, application.Status);
        Assert.Equal("We do not serve this destination yet.", application.DeclineReason);
        Assert.Equal(AdminUserId, application.ReviewedByUserId);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Decline_RefusesAnAcceptedApplication()
    {
        var (applications, unitOfWork) = Build();
        var application = applications.Add(StudentUserId);
        application.Accept(AdminUserId, new FakeClock().UtcNow);

        var result = await Decline(applications, unitOfWork).Handle(
            new DeclineAcademyApplicationCommand(application.Id, "Changed my mind"),
            default);

        Assert.Equal(AcademyApplicationErrors.NotSubmitted, result.Error);
        Assert.Equal(AcademyApplicationStatus.Accepted, application.Status);
        Assert.Null(application.DeclineReason);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Decline_FailsForAnUnknownApplication()
    {
        var (applications, unitOfWork) = Build();

        var result = await Decline(applications, unitOfWork).Handle(
            new DeclineAcademyApplicationCommand(Guid.NewGuid(), "Reason"),
            default);

        Assert.Equal(AcademyApplicationErrors.NotFound, result.Error);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Withdraw_LetsTheStudentWithdrawASubmittedApplication()
    {
        var (applications, unitOfWork) = Build();
        var application = applications.Add(StudentUserId);

        var result = await Withdraw(applications, unitOfWork, StudentUserId).Handle(
            new WithdrawAcademyApplicationCommand(application.Id),
            default);

        Assert.True(result.IsSuccess);
        Assert.Equal(AcademyApplicationStatus.Withdrawn, application.Status);
        Assert.Equal(new FakeClock().UtcNow, application.WithdrawnOnUtc);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Withdraw_HidesAnotherStudentsApplication()
    {
        var (applications, unitOfWork) = Build();
        var application = applications.Add(StudentUserId);

        var result = await Withdraw(applications, unitOfWork, Guid.NewGuid()).Handle(
            new WithdrawAcademyApplicationCommand(application.Id),
            default);

        Assert.Equal(AcademyApplicationErrors.NotFound, result.Error);
        Assert.Equal(AcademyApplicationStatus.Submitted, application.Status);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Withdraw_RefusesAnAcceptedApplication()
    {
        var (applications, unitOfWork) = Build();
        var application = applications.Add(StudentUserId);
        application.Accept(AdminUserId, new FakeClock().UtcNow);

        var result = await Withdraw(applications, unitOfWork, StudentUserId).Handle(
            new WithdrawAcademyApplicationCommand(application.Id),
            default);

        Assert.Equal(AcademyApplicationErrors.NotSubmitted, result.Error);
        Assert.Equal(AcademyApplicationStatus.Accepted, application.Status);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void DeclineValidator_WhenTheReasonIsEmpty_Fails(string reason)
    {
        var result = new DeclineAcademyApplicationCommandValidator().Validate(
            new DeclineAcademyApplicationCommand(Guid.NewGuid(), reason));

        Assert.Contains(result.Errors, error =>
            error.PropertyName == "Reason" && error.ErrorCode == AcademyApplicationErrors.DeclineReasonRequired.Code);
    }

    [Fact]
    public void DeclineValidator_WhenTheReasonIsTooLong_Fails()
    {
        var result = new DeclineAcademyApplicationCommandValidator().Validate(
            new DeclineAcademyApplicationCommand(Guid.NewGuid(), new string('a', 501)));

        Assert.Contains(result.Errors, error =>
            error.PropertyName == "Reason" && error.ErrorCode == AcademyApplicationErrors.DeclineReasonRequired.Code);
    }

    [Fact]
    public void DeclineValidator_WhenTheIdIsEmpty_Fails()
    {
        var result = new DeclineAcademyApplicationCommandValidator().Validate(
            new DeclineAcademyApplicationCommand(Guid.Empty, "Reason"));

        Assert.Contains(result.Errors, error => error.PropertyName == "Id");
    }

    [Fact]
    public void DeclineValidator_AcceptsAReason()
    {
        var result = new DeclineAcademyApplicationCommandValidator().Validate(
            new DeclineAcademyApplicationCommand(Guid.NewGuid(), "We do not serve this destination yet."));

        Assert.True(result.IsValid);
    }

    private static (FakeAcademyApplicationRepository Applications, FakeUnitOfWork UnitOfWork) Build() =>
        (new FakeAcademyApplicationRepository(), new FakeUnitOfWork());

    private static AcceptAcademyApplicationCommandHandler Accept(FakeAcademyApplicationRepository applications, FakeUnitOfWork unitOfWork) =>
        new(unitOfWork, new FakeCurrentUser(AdminUserId), applications, new FakeClock());

    private static DeclineAcademyApplicationCommandHandler Decline(FakeAcademyApplicationRepository applications, FakeUnitOfWork unitOfWork) =>
        new(unitOfWork, new FakeCurrentUser(AdminUserId), applications, new FakeClock());

    private static WithdrawAcademyApplicationCommandHandler Withdraw(
        FakeAcademyApplicationRepository applications,
        FakeUnitOfWork unitOfWork,
        Guid currentUserId) =>
        new(unitOfWork, new FakeCurrentUser(currentUserId), applications, new FakeClock());
}
