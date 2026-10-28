namespace FourPrep.Modules.Academy.Domain.Enrolments;

using FourPrep.Common.Domain;

public sealed class Enrolment
{
    public const int MaxUnpaidPerStudent = 3;

    public static readonly TimeSpan PaymentWindow = TimeSpan.FromHours(24);

    public Guid Id { get; set; }
    public Guid StudentUserId { get; set; }
    public Guid EnrolledByUserId { get; set; }
    public EnrolmentStatus Status { get; set; }
    public EnrolmentDropReason? DropReason { get; set; }
    public DateTimeOffset? PaymentDueOnUtc { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset UpdatedOnUtc { get; set; }

    public bool HoldsSeat => Status is EnrolmentStatus.PendingPayment or EnrolmentStatus.Active;

    public bool IsCurrent => HoldsSeat || Status == EnrolmentStatus.Waitlisted;

    internal static Enrolment Create(Guid studentUserId, Guid enrolledByUserId, bool seatAvailable, DateTime utcNow)
    {
        var enrolment = new Enrolment
        {
            Id = Guid.NewGuid(),
            StudentUserId = studentUserId,
            EnrolledByUserId = enrolledByUserId,
            Status = EnrolmentStatus.Waitlisted,
            CreatedOnUtc = utcNow,
            UpdatedOnUtc = utcNow
        };

        if (seatAvailable)
        {
            enrolment.OfferSeat(utcNow);
        }

        return enrolment;
    }

    internal bool IsPaymentOverdue(DateTime utcNow) =>
        Status == EnrolmentStatus.PendingPayment && PaymentDueOnUtc <= utcNow;

    internal void OfferSeat(DateTime utcNow)
    {
        Status = EnrolmentStatus.PendingPayment;
        PaymentDueOnUtc = utcNow + PaymentWindow;
        UpdatedOnUtc = utcNow;
    }

    internal Result Activate(DateTime utcNow)
    {
        if (Status != EnrolmentStatus.PendingPayment)
        {
            return Result.Failure(EnrolmentErrors.NotPendingPayment);
        }

        Status = EnrolmentStatus.Active;
        PaymentDueOnUtc = null;
        UpdatedOnUtc = utcNow;

        return Result.Success();
    }

    internal Result Cancel(DateTime utcNow)
    {
        if (Status is not (EnrolmentStatus.Waitlisted or EnrolmentStatus.PendingPayment))
        {
            return Result.Failure(EnrolmentErrors.CannotCancel);
        }

        Drop(EnrolmentDropReason.CancelledByFamily, utcNow);

        return Result.Success();
    }

    internal Result Remove(DateTime utcNow)
    {
        if (!IsCurrent)
        {
            return Result.Failure(EnrolmentErrors.AlreadyEnded);
        }

        Drop(EnrolmentDropReason.RemovedByStaff, utcNow);

        return Result.Success();
    }

    internal void Expire(DateTime utcNow) => Drop(EnrolmentDropReason.PaymentExpired, utcNow);

    internal void EndWithGroup(DateTime utcNow) => Drop(EnrolmentDropReason.GroupCancelled, utcNow);

    private void Drop(EnrolmentDropReason reason, DateTime utcNow)
    {
        Status = EnrolmentStatus.Dropped;
        DropReason = reason;
        PaymentDueOnUtc = null;
        UpdatedOnUtc = utcNow;
    }
}
