namespace FourPrep.Modules.Academy.Application.Enrolments.GetEnrolments;

using FourPrep.Modules.Academy.Domain.ClassGroups;
using FourPrep.Modules.Academy.Domain.Enrolments;

public sealed record EnrolmentResponse
{
    public Guid Id { get; init; }
    public Guid ClassGroupId { get; init; }
    public string ClassGroupCode { get; init; } = string.Empty;
    public string CourseSlug { get; init; } = string.Empty;
    public Dictionary<string, string> CourseName { get; init; } = [];
    public ClassMode Mode { get; init; }
    public DateOnly StartsOn { get; init; }
    public Guid StudentUserId { get; init; }
    public Guid? EnrolledByUserId { get; init; }
    public bool EnrolledByMe { get; init; }
    public EnrolmentStatus Status { get; init; }
    public EnrolmentDropReason? DropReason { get; init; }
    public DateTime? PaymentDueOnUtc { get; init; }
    public int? WaitlistPosition { get; init; }
    public DateTime CreatedOnUtc { get; init; }

    internal EnrolmentResponse ForFamily(Guid currentUserId) =>
        this with { EnrolledByMe = EnrolledByUserId == currentUserId, EnrolledByUserId = null };
}
