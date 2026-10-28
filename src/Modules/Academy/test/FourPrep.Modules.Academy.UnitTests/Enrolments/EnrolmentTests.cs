namespace FourPrep.Modules.Academy.UnitTests.Enrolments;

using FourPrep.Modules.Academy.Domain.ClassGroups;
using FourPrep.Modules.Academy.Domain.Enrolments;

public class EnrolmentTests
{
    private static readonly DateTime Start = new(2026, 10, 29, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Enrol_HoldsAFreeSeatForTheFullPaymentWindow()
    {
        var group = Group(capacity: 1);

        var enrolment = group.Enrol(Guid.NewGuid(), Guid.NewGuid(), Start).Data;

        Assert.Equal(EnrolmentStatus.PendingPayment, enrolment.Status);
        Assert.Equal(Start + Enrolment.PaymentWindow, enrolment.PaymentDueOnUtc);
        Assert.Equal(1, group.SeatsTaken);
    }

    [Fact]
    public void Enrol_PutsTheStudentOnTheWaitlistWhenTheGroupIsFull()
    {
        var group = Group(capacity: 1);
        group.Enrol(Guid.NewGuid(), Guid.NewGuid(), Start);

        var enrolment = group.Enrol(Guid.NewGuid(), Guid.NewGuid(), Start.AddMinutes(1)).Data;

        Assert.Equal(EnrolmentStatus.Waitlisted, enrolment.Status);
        Assert.Null(enrolment.PaymentDueOnUtc);
        Assert.Equal(1, group.SeatsTaken);
    }

    [Theory]
    [InlineData(ClassGroupStatus.Draft)]
    [InlineData(ClassGroupStatus.Closed)]
    [InlineData(ClassGroupStatus.Cancelled)]
    public void Enrol_RefusesAGroupThatIsNotOpen(ClassGroupStatus status)
    {
        var group = Group(capacity: 5);
        group.Status = status;

        var result = group.Enrol(Guid.NewGuid(), Guid.NewGuid(), Start);

        Assert.Equal(ClassGroupErrors.NotOpen, result.Error);
        Assert.Empty(group.Enrolments);
    }

    [Fact]
    public void Enrol_RefusesAStudentWhoIsAlreadyInTheGroupOrOnItsWaitlist()
    {
        var group = Group(capacity: 1);
        var student = Guid.NewGuid();
        group.Enrol(Guid.NewGuid(), Guid.NewGuid(), Start);
        group.Enrol(student, student, Start.AddMinutes(1));

        var result = group.Enrol(student, student, Start.AddMinutes(2));

        Assert.Equal(EnrolmentErrors.AlreadyEnrolled, result.Error);
        Assert.Equal(2, group.Enrolments.Count);
    }

    [Fact]
    public void Enrol_LetsAStudentComeBackAfterCancelling()
    {
        var group = Group(capacity: 1);
        var student = Guid.NewGuid();
        var first = group.Enrol(student, student, Start).Data;
        group.CancelEnrolment(first.Id, Start.AddMinutes(1));

        var second = group.Enrol(student, student, Start.AddMinutes(2));

        Assert.True(second.IsSuccess);
        Assert.Equal(EnrolmentStatus.PendingPayment, second.Data.Status);
    }

    [Fact]
    public void Cancel_GivesTheFreedSeatToTheFirstPersonOnTheWaitlist()
    {
        var group = Group(capacity: 1);
        var holder = group.Enrol(Guid.NewGuid(), Guid.NewGuid(), Start).Data;
        var first = group.Enrol(Guid.NewGuid(), Guid.NewGuid(), Start.AddMinutes(1)).Data;
        var second = group.Enrol(Guid.NewGuid(), Guid.NewGuid(), Start.AddMinutes(2)).Data;
        var cancelledAt = Start.AddHours(3);

        var result = group.CancelEnrolment(holder.Id, cancelledAt);

        Assert.True(result.IsSuccess);
        Assert.Equal(EnrolmentStatus.Dropped, holder.Status);
        Assert.Equal(EnrolmentDropReason.CancelledByFamily, holder.DropReason);
        Assert.Equal(EnrolmentStatus.PendingPayment, first.Status);
        Assert.Equal(cancelledAt + Enrolment.PaymentWindow, first.PaymentDueOnUtc);
        Assert.Equal(EnrolmentStatus.Waitlisted, second.Status);
    }

    [Fact]
    public void Cancel_LeavesTheSeatsAloneWhenAWaitlistedStudentLeaves()
    {
        var group = Group(capacity: 1);
        var holder = group.Enrol(Guid.NewGuid(), Guid.NewGuid(), Start).Data;
        var waiting = group.Enrol(Guid.NewGuid(), Guid.NewGuid(), Start.AddMinutes(1)).Data;

        group.CancelEnrolment(waiting.Id, Start.AddMinutes(2));

        Assert.Equal(EnrolmentStatus.Dropped, waiting.Status);
        Assert.Equal(EnrolmentStatus.PendingPayment, holder.Status);
    }

    [Fact]
    public void Cancel_RefusesAnActiveEnrolment()
    {
        var group = Group(capacity: 1);
        var enrolment = group.Enrol(Guid.NewGuid(), Guid.NewGuid(), Start).Data;
        group.ActivateEnrolment(enrolment.Id, Start.AddMinutes(1));

        var result = group.CancelEnrolment(enrolment.Id, Start.AddMinutes(2));

        Assert.Equal(EnrolmentErrors.CannotCancel, result.Error);
        Assert.Equal(EnrolmentStatus.Active, enrolment.Status);
    }

    [Fact]
    public void Activate_ConfirmsAnEnrolmentWaitingForPayment()
    {
        var group = Group(capacity: 1);
        var enrolment = group.Enrol(Guid.NewGuid(), Guid.NewGuid(), Start).Data;

        var result = group.ActivateEnrolment(enrolment.Id, Start.AddHours(1));

        Assert.True(result.IsSuccess);
        Assert.Equal(EnrolmentStatus.Active, enrolment.Status);
        Assert.Null(enrolment.PaymentDueOnUtc);
        Assert.Equal(1, group.SeatsTaken);
    }

    [Fact]
    public void Activate_RefusesAWaitlistedEnrolment()
    {
        var group = Group(capacity: 1);
        group.Enrol(Guid.NewGuid(), Guid.NewGuid(), Start);
        var waiting = group.Enrol(Guid.NewGuid(), Guid.NewGuid(), Start.AddMinutes(1)).Data;

        var result = group.ActivateEnrolment(waiting.Id, Start.AddMinutes(2));

        Assert.Equal(EnrolmentErrors.NotPendingPayment, result.Error);
        Assert.Equal(EnrolmentStatus.Waitlisted, waiting.Status);
    }

    [Fact]
    public void Remove_DropsAnActiveEnrolmentAndOffersTheSeat()
    {
        var group = Group(capacity: 1);
        var holder = group.Enrol(Guid.NewGuid(), Guid.NewGuid(), Start).Data;
        group.ActivateEnrolment(holder.Id, Start.AddMinutes(1));
        var waiting = group.Enrol(Guid.NewGuid(), Guid.NewGuid(), Start.AddMinutes(2)).Data;

        var result = group.RemoveEnrolment(holder.Id, Start.AddMinutes(3));

        Assert.True(result.IsSuccess);
        Assert.Equal(EnrolmentDropReason.RemovedByStaff, holder.DropReason);
        Assert.Equal(EnrolmentStatus.PendingPayment, waiting.Status);
    }

    [Fact]
    public void Remove_RefusesAnEnrolmentThatHasEnded()
    {
        var group = Group(capacity: 1);
        var enrolment = group.Enrol(Guid.NewGuid(), Guid.NewGuid(), Start).Data;
        group.CancelEnrolment(enrolment.Id, Start.AddMinutes(1));

        var result = group.RemoveEnrolment(enrolment.Id, Start.AddMinutes(2));

        Assert.Equal(EnrolmentErrors.AlreadyEnded, result.Error);
        Assert.Equal(EnrolmentDropReason.CancelledByFamily, enrolment.DropReason);
    }

    [Fact]
    public void AnEnrolmentChange_ReportsAnUnknownEnrolment()
    {
        var group = Group(capacity: 1);

        Assert.Equal(EnrolmentErrors.NotFound, group.ActivateEnrolment(Guid.NewGuid(), Start).Error);
        Assert.Equal(EnrolmentErrors.NotFound, group.CancelEnrolment(Guid.NewGuid(), Start).Error);
        Assert.Equal(EnrolmentErrors.NotFound, group.RemoveEnrolment(Guid.NewGuid(), Start).Error);
    }

    [Fact]
    public void ExpireUnpaidEnrolments_DropsOverdueSeatsAndOffersThemInWaitlistOrder()
    {
        var group = Group(capacity: 2);
        var unpaid = group.Enrol(Guid.NewGuid(), Guid.NewGuid(), Start).Data;
        var paid = group.Enrol(Guid.NewGuid(), Guid.NewGuid(), Start.AddMinutes(1)).Data;
        group.ActivateEnrolment(paid.Id, Start.AddMinutes(2));
        var second = group.Enrol(Guid.NewGuid(), Guid.NewGuid(), Start.AddMinutes(4)).Data;
        var first = group.Enrol(Guid.NewGuid(), Guid.NewGuid(), Start.AddMinutes(3)).Data;
        var afterDeadline = Start + Enrolment.PaymentWindow + TimeSpan.FromMinutes(1);

        var expired = group.ExpireUnpaidEnrolments(afterDeadline);

        Assert.Equal(1, expired);
        Assert.Equal(EnrolmentDropReason.PaymentExpired, unpaid.DropReason);
        Assert.Equal(EnrolmentStatus.Active, paid.Status);
        Assert.Equal(EnrolmentStatus.PendingPayment, first.Status);
        Assert.Equal(EnrolmentStatus.Waitlisted, second.Status);
    }

    [Fact]
    public void ExpireUnpaidEnrolments_KeepsASeatWhosePaymentIsNotDueYet()
    {
        var group = Group(capacity: 1);
        var enrolment = group.Enrol(Guid.NewGuid(), Guid.NewGuid(), Start).Data;

        var expired = group.ExpireUnpaidEnrolments(Start + Enrolment.PaymentWindow - TimeSpan.FromMinutes(1));

        Assert.Equal(0, expired);
        Assert.Equal(EnrolmentStatus.PendingPayment, enrolment.Status);
    }

    [Fact]
    public void Update_OffersNewSeatsToTheWaitlistWhenTheCapacityGrows()
    {
        var group = Group(capacity: 1);
        group.Enrol(Guid.NewGuid(), Guid.NewGuid(), Start);
        var first = group.Enrol(Guid.NewGuid(), Guid.NewGuid(), Start.AddMinutes(1)).Data;
        var second = group.Enrol(Guid.NewGuid(), Guid.NewGuid(), Start.AddMinutes(2)).Data;

        (ClassGroupDetailsSamples.Online() with { Capacity = 3 }).ApplyTo(group, Start.AddHours(1));

        Assert.Equal(EnrolmentStatus.PendingPayment, first.Status);
        Assert.Equal(EnrolmentStatus.PendingPayment, second.Status);
        Assert.Equal(3, group.SeatsTaken);
    }

    [Fact]
    public void CancellingTheGroup_EndsEveryCurrentEnrolment()
    {
        var group = Group(capacity: 2);
        var active = group.Enrol(Guid.NewGuid(), Guid.NewGuid(), Start).Data;
        group.ActivateEnrolment(active.Id, Start.AddMinutes(1));
        var pending = group.Enrol(Guid.NewGuid(), Guid.NewGuid(), Start.AddMinutes(2)).Data;
        var waiting = group.Enrol(Guid.NewGuid(), Guid.NewGuid(), Start.AddMinutes(3)).Data;
        var left = group.Enrol(Guid.NewGuid(), Guid.NewGuid(), Start.AddMinutes(4)).Data;
        group.CancelEnrolment(left.Id, Start.AddMinutes(5));

        group.ChangeStatus(ClassGroupStatus.Cancelled, Start.AddHours(1));

        Assert.All([active, pending, waiting], enrolment =>
        {
            Assert.Equal(EnrolmentStatus.Dropped, enrolment.Status);
            Assert.Equal(EnrolmentDropReason.GroupCancelled, enrolment.DropReason);
        });
        Assert.Equal(EnrolmentDropReason.CancelledByFamily, left.DropReason);
        Assert.Equal(0, group.SeatsTaken);
    }

    [Fact]
    public void CancellingTheGroup_LeavesNothingToActivateOrOffer()
    {
        var group = Group(capacity: 1);
        var pending = group.Enrol(Guid.NewGuid(), Guid.NewGuid(), Start).Data;
        group.ChangeStatus(ClassGroupStatus.Cancelled, Start.AddMinutes(1));

        var activated = group.ActivateEnrolment(pending.Id, Start.AddMinutes(2));
        var expired = group.ExpireUnpaidEnrolments(Start + Enrolment.PaymentWindow + TimeSpan.FromHours(1));

        Assert.Equal(EnrolmentErrors.NotPendingPayment, activated.Error);
        Assert.Equal(0, expired);
        Assert.Equal(0, group.SeatsTaken);
    }

    private static ClassGroup Group(int capacity) => new FakeClassGroupRepository().AddOpen(capacity);
}
