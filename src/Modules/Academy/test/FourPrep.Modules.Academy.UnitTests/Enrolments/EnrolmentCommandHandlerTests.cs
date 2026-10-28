namespace FourPrep.Modules.Academy.UnitTests.Enrolments;

using FourPrep.Common.Domain;
using FourPrep.Modules.Academy.Domain.ClassGroups;
using FourPrep.Modules.Academy.Domain.Enrolments;
using FourPrep.Modules.Academy.Application.Enrolments.EnrolStudent;
using FourPrep.Modules.Academy.Application.Enrolments.GetEnrolments;
using FourPrep.Modules.Academy.Application.Enrolments.CancelEnrolment;
using FourPrep.Modules.Academy.Application.Enrolments.RemoveEnrolment;
using FourPrep.Modules.Academy.Application.Enrolments.ActivateEnrolment;
using FourPrep.Modules.Academy.Application.ClassGroups.UpdateClassGroup;
using FourPrep.Modules.Academy.Application.Enrolments.ExpireUnpaidEnrolments;

public class EnrolmentCommandHandlerTests
{
    private static readonly Guid StudentId = Guid.NewGuid();
    private static readonly Guid ParentId = Guid.NewGuid();

    [Fact]
    public async Task Enrol_LetsAStudentEnrolThemself()
    {
        var (groups, students, unitOfWork) = Build();
        var group = groups.AddOpen(capacity: 5);
        students.Access.Add((StudentId, StudentId));

        var result = await EnrolHandler(groups, students, unitOfWork, StudentId)
            .Handle(new EnrolStudentCommand(group.Id, StudentId), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(EnrolmentStatus.PendingPayment, result.Data.Status);
        Assert.Equal(new FakeClock().UtcNow + Enrolment.PaymentWindow, result.Data.PaymentDueOnUtc);
        var enrolment = Assert.Single(group.Enrolments);
        Assert.Equal(StudentId, enrolment.StudentUserId);
        Assert.Equal(StudentId, enrolment.EnrolledByUserId);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Enrol_LetsALinkedParentEnrolTheirChild()
    {
        var (groups, students, unitOfWork) = Build();
        var group = groups.AddOpen(capacity: 5);
        students.Access.Add((ParentId, StudentId));

        var result = await EnrolHandler(groups, students, unitOfWork, ParentId)
            .Handle(new EnrolStudentCommand(group.Id, StudentId), default);

        Assert.True(result.IsSuccess);
        var enrolment = Assert.Single(group.Enrolments);
        Assert.Equal(StudentId, enrolment.StudentUserId);
        Assert.Equal(ParentId, enrolment.EnrolledByUserId);
    }

    [Fact]
    public async Task Enrol_RefusesSomeoneWhoIsNotTheStudentOrALinkedParent()
    {
        var (groups, students, unitOfWork) = Build();
        var group = groups.AddOpen(capacity: 5);
        students.Access.Add((ParentId, Guid.NewGuid()));

        var result = await EnrolHandler(groups, students, unitOfWork, ParentId)
            .Handle(new EnrolStudentCommand(group.Id, StudentId), default);

        Assert.Equal(EnrolmentErrors.NotYourStudent, result.Error);
        Assert.Empty(group.Enrolments);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Enrol_PassesOnTheStudentModuleFailure()
    {
        var failure = Error.Failure("Student.Unavailable", en: "Down.", uz: "Ishlamayapti.", ru: "Недоступно.");
        var groups = new FakeClassGroupRepository();
        var unitOfWork = new FakeUnitOfWork();
        var group = groups.AddOpen(capacity: 5);

        var result = await EnrolHandler(groups, new FakeStudentService { Failure = failure }, unitOfWork, StudentId)
            .Handle(new EnrolStudentCommand(group.Id, StudentId), default);

        Assert.Equal(failure, result.Error);
        Assert.Empty(group.Enrolments);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Enrol_RefusesAGroupThatIsNotOpen()
    {
        var (groups, students, unitOfWork) = Build();
        var group = groups.Add(Guid.NewGuid());
        students.Access.Add((StudentId, StudentId));

        var result = await EnrolHandler(groups, students, unitOfWork, StudentId)
            .Handle(new EnrolStudentCommand(group.Id, StudentId), default);

        Assert.Equal(ClassGroupErrors.NotOpen, result.Error);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Enrol_RefusesAFourthUnpaidEnrolment()
    {
        var (groups, students, unitOfWork) = Build();
        students.Access.Add((StudentId, StudentId));
        for (var i = 0; i < Enrolment.MaxUnpaidPerStudent; i++)
        {
            groups.AddOpen(capacity: 5).Enrol(StudentId, StudentId, new FakeClock().UtcNow);
        }

        var next = groups.AddOpen(capacity: 5);

        var result = await EnrolHandler(groups, students, unitOfWork, StudentId)
            .Handle(new EnrolStudentCommand(next.Id, StudentId), default);

        Assert.Equal(EnrolmentErrors.TooManyUnpaid, result.Error);
        Assert.Empty(next.Enrolments);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Enrol_DoesNotCountPaidEnrolmentsAgainstTheLimit()
    {
        var (groups, students, unitOfWork) = Build();
        students.Access.Add((StudentId, StudentId));
        for (var i = 0; i < Enrolment.MaxUnpaidPerStudent; i++)
        {
            var paidGroup = groups.AddOpen(capacity: 5);
            var paid = paidGroup.Enrol(StudentId, StudentId, new FakeClock().UtcNow).Data;
            paidGroup.ActivateEnrolment(paid.Id, new FakeClock().UtcNow);
        }

        var next = groups.AddOpen(capacity: 5);

        var result = await EnrolHandler(groups, students, unitOfWork, StudentId)
            .Handle(new EnrolStudentCommand(next.Id, StudentId), default);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Enrol_ReportsAnUnknownGroup()
    {
        var (groups, students, unitOfWork) = Build();

        var result = await EnrolHandler(groups, students, unitOfWork, StudentId)
            .Handle(new EnrolStudentCommand(Guid.NewGuid(), StudentId), default);

        Assert.Equal(ClassGroupErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Cancel_LetsTheFamilyCancelAWaitingEnrolment()
    {
        var (groups, students, unitOfWork) = Build();
        var (group, enrolment) = Enrolled(groups);
        students.Access.Add((ParentId, StudentId));

        var result = await CancelHandler(groups, students, unitOfWork, ParentId)
            .Handle(new CancelEnrolmentCommand(enrolment.Id), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(EnrolmentStatus.Dropped, enrolment.Status);
        Assert.Equal(0, group.SeatsTaken);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Cancel_HidesAnotherFamilysEnrolment()
    {
        var (groups, students, unitOfWork) = Build();
        var (_, enrolment) = Enrolled(groups);

        var result = await CancelHandler(groups, students, unitOfWork, ParentId)
            .Handle(new CancelEnrolmentCommand(enrolment.Id), default);

        Assert.Equal(EnrolmentErrors.NotFound, result.Error);
        Assert.Equal(EnrolmentStatus.PendingPayment, enrolment.Status);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Cancel_PassesOnTheStudentModuleFailure()
    {
        var failure = Error.Failure("Student.Unavailable", en: "Down.", uz: "Ishlamayapti.", ru: "Недоступно.");
        var groups = new FakeClassGroupRepository();
        var unitOfWork = new FakeUnitOfWork();
        var (_, enrolment) = Enrolled(groups);

        var result = await CancelHandler(groups, new FakeStudentService { Failure = failure }, unitOfWork, StudentId)
            .Handle(new CancelEnrolmentCommand(enrolment.Id), default);

        Assert.Equal(failure, result.Error);
        Assert.Equal(EnrolmentStatus.PendingPayment, enrolment.Status);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Cancel_RefusesAnActiveEnrolment()
    {
        var (groups, students, unitOfWork) = Build();
        var (group, enrolment) = Enrolled(groups);
        group.ActivateEnrolment(enrolment.Id, new FakeClock().UtcNow);
        students.Access.Add((StudentId, StudentId));

        var result = await CancelHandler(groups, students, unitOfWork, StudentId)
            .Handle(new CancelEnrolmentCommand(enrolment.Id), default);

        Assert.Equal(EnrolmentErrors.CannotCancel, result.Error);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Cancel_ReportsAnUnknownEnrolment()
    {
        var (groups, students, unitOfWork) = Build();

        var result = await CancelHandler(groups, students, unitOfWork, StudentId)
            .Handle(new CancelEnrolmentCommand(Guid.NewGuid()), default);

        Assert.Equal(EnrolmentErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Activate_ConfirmsTheSeat()
    {
        var (groups, _, unitOfWork) = Build();
        var (_, enrolment) = Enrolled(groups);

        var result = await new ActivateEnrolmentCommandHandler(unitOfWork, groups, new FakeClock())
            .Handle(new ActivateEnrolmentCommand(enrolment.Id), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(EnrolmentStatus.Active, enrolment.Status);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Activate_RefusesAnEnrolmentThatIsNotWaitingForPayment()
    {
        var (groups, _, unitOfWork) = Build();
        var (group, enrolment) = Enrolled(groups);
        group.CancelEnrolment(enrolment.Id, new FakeClock().UtcNow);

        var result = await new ActivateEnrolmentCommandHandler(unitOfWork, groups, new FakeClock())
            .Handle(new ActivateEnrolmentCommand(enrolment.Id), default);

        Assert.Equal(EnrolmentErrors.NotPendingPayment, result.Error);
        Assert.Equal(EnrolmentStatus.Dropped, enrolment.Status);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Activate_ReportsAnUnknownEnrolment()
    {
        var (groups, _, unitOfWork) = Build();

        var result = await new ActivateEnrolmentCommandHandler(unitOfWork, groups, new FakeClock())
            .Handle(new ActivateEnrolmentCommand(Guid.NewGuid()), default);

        Assert.Equal(EnrolmentErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Remove_DropsTheEnrolment()
    {
        var (groups, _, unitOfWork) = Build();
        var (_, enrolment) = Enrolled(groups);

        var result = await new RemoveEnrolmentCommandHandler(unitOfWork, groups, new FakeClock())
            .Handle(new RemoveEnrolmentCommand(enrolment.Id), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(EnrolmentDropReason.RemovedByStaff, enrolment.DropReason);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Remove_RefusesAnEnrolmentThatHasEnded()
    {
        var (groups, _, unitOfWork) = Build();
        var (group, enrolment) = Enrolled(groups);
        group.CancelEnrolment(enrolment.Id, new FakeClock().UtcNow);

        var result = await new RemoveEnrolmentCommandHandler(unitOfWork, groups, new FakeClock())
            .Handle(new RemoveEnrolmentCommand(enrolment.Id), default);

        Assert.Equal(EnrolmentErrors.AlreadyEnded, result.Error);
        Assert.Equal(EnrolmentDropReason.CancelledByFamily, enrolment.DropReason);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Remove_ReportsAnUnknownEnrolment()
    {
        var (groups, _, unitOfWork) = Build();

        var result = await new RemoveEnrolmentCommandHandler(unitOfWork, groups, new FakeClock())
            .Handle(new RemoveEnrolmentCommand(Guid.NewGuid()), default);

        Assert.Equal(EnrolmentErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Expire_DropsOverdueSeatsAndSavesOnce()
    {
        var (groups, _, unitOfWork) = Build();
        var (_, enrolment) = Enrolled(groups, enrolledAt: new FakeClock().UtcNow - Enrolment.PaymentWindow - TimeSpan.FromHours(1));
        Enrolled(groups, enrolledAt: new FakeClock().UtcNow - Enrolment.PaymentWindow - TimeSpan.FromHours(2));

        var result = await new ExpireUnpaidEnrolmentsCommandHandler(unitOfWork, groups, new FakeClock())
            .Handle(new ExpireUnpaidEnrolmentsCommand(), default);

        Assert.Equal(2, result.Data);
        Assert.Equal(EnrolmentDropReason.PaymentExpired, enrolment.DropReason);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Expire_SavesNothingWhenNoPaymentIsOverdue()
    {
        var (groups, _, unitOfWork) = Build();
        Enrolled(groups);

        var result = await new ExpireUnpaidEnrolmentsCommandHandler(unitOfWork, groups, new FakeClock())
            .Handle(new ExpireUnpaidEnrolmentsCommand(), default);

        Assert.Equal(0, result.Data);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task UpdateGroup_RefusesFewerSeatsThanStudentsHolding()
    {
        var (groups, _, unitOfWork) = Build();
        var group = groups.AddOpen(capacity: 2);
        group.Enrol(Guid.NewGuid(), Guid.NewGuid(), new FakeClock().UtcNow);
        group.Enrol(Guid.NewGuid(), Guid.NewGuid(), new FakeClock().UtcNow);
        var details = ClassGroupDetailsSamples.Online() with { Code = group.Code, Capacity = 1 };

        var result = await new UpdateClassGroupCommandHandler(unitOfWork, groups, new FakeClock())
            .Handle(new UpdateClassGroupCommand(group.Id, details), default);

        Assert.Equal(ClassGroupErrors.CapacityBelowSeatsTaken, result.Error);
        Assert.Equal(2, group.Capacity);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task GetStudentEnrolments_RefusesSomeoneOutsideTheFamilyBeforeReading()
    {
        var students = new FakeStudentService();

        var result = await new GetStudentEnrolmentsQueryHandler(new FakeCurrentUser(ParentId), students, new UnusedConnectionFactory())
            .Handle(new GetStudentEnrolmentsQuery(StudentId), default);

        Assert.Equal(EnrolmentErrors.NotYourStudent, result.Error);
    }

    [Fact]
    public void ForFamily_SaysWhoEnrolledWithoutShowingAnotherParentsAccount()
    {
        var response = new EnrolmentResponse { Id = Guid.NewGuid(), EnrolledByUserId = ParentId };

        var mine = response.ForFamily(ParentId);
        var theirs = response.ForFamily(StudentId);

        Assert.True(mine.EnrolledByMe);
        Assert.False(theirs.EnrolledByMe);
        Assert.Null(mine.EnrolledByUserId);
        Assert.Null(theirs.EnrolledByUserId);
    }

    [Fact]
    public void Validate_WhenTheEnrolmentIsComplete_Passes()
    {
        Assert.True(new EnrolStudentCommandValidator().Validate(new EnrolStudentCommand(Guid.NewGuid(), StudentId)).IsValid);
        Assert.True(new GetStudentEnrolmentsQueryValidator().Validate(new GetStudentEnrolmentsQuery(StudentId)).IsValid);
    }

    [Fact]
    public void Validate_WhenTheStudentToListIsMissing_Fails()
    {
        var error = Assert.Single(new GetStudentEnrolmentsQueryValidator().Validate(new GetStudentEnrolmentsQuery(Guid.Empty)).Errors);

        Assert.Equal(nameof(GetStudentEnrolmentsQuery.StudentUserId), error.PropertyName);
        Assert.Equal("NotEmptyValidator", error.ErrorCode);
    }

    [Fact]
    public void Validate_WhenTheGroupOrStudentIsMissing_Fails()
    {
        var errors = new EnrolStudentCommandValidator()
            .Validate(new EnrolStudentCommand(Guid.Empty, Guid.Empty)).Errors;

        Assert.Equal(
            [nameof(EnrolStudentCommand.ClassGroupId), nameof(EnrolStudentCommand.StudentUserId)],
            errors.Select(error => error.PropertyName));
        Assert.All(errors, error => Assert.Equal("NotEmptyValidator", error.ErrorCode));
    }

    private static (FakeClassGroupRepository, FakeStudentService, FakeUnitOfWork) Build() =>
        (new FakeClassGroupRepository(), new FakeStudentService(), new FakeUnitOfWork());

    private static (ClassGroup, Enrolment) Enrolled(FakeClassGroupRepository groups, DateTime? enrolledAt = null)
    {
        var group = groups.AddOpen(capacity: 5);
        var enrolment = group.Enrol(StudentId, StudentId, enrolledAt ?? new FakeClock().UtcNow).Data;
        return (group, enrolment);
    }

    private static EnrolStudentCommandHandler EnrolHandler(
        FakeClassGroupRepository groups,
        FakeStudentService students,
        FakeUnitOfWork unitOfWork,
        Guid currentUserId) =>
        new(unitOfWork, new FakeCurrentUser(currentUserId), students, groups, new FakeClock());

    private static CancelEnrolmentCommandHandler CancelHandler(
        FakeClassGroupRepository groups,
        FakeStudentService students,
        FakeUnitOfWork unitOfWork,
        Guid currentUserId) =>
        new(unitOfWork, new FakeCurrentUser(currentUserId), students, groups, new FakeClock());
}
