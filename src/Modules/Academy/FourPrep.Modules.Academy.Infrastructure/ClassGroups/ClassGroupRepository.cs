namespace FourPrep.Modules.Academy.Infrastructure.ClassGroups;

using Microsoft.EntityFrameworkCore;
using FourPrep.Modules.Academy.Domain.ClassGroups;
using FourPrep.Modules.Academy.Domain.Enrolments;
using FourPrep.Modules.Academy.Infrastructure.Database;

internal sealed class ClassGroupRepository(AcademyDbContext dbContext) : IClassGroupRepository
{
    public async Task<ClassGroup?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.ClassGroups.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<ClassGroup?> GetByEnrolmentIdAsync(Guid enrolmentId, CancellationToken cancellationToken = default)
    {
        return await dbContext.ClassGroups.SingleOrDefaultAsync(
            x => x.Enrolments.Any(enrolment => enrolment.Id == enrolmentId),
            cancellationToken);
    }

    public async Task<IReadOnlyList<ClassGroup>> GetWithPaymentsDueAsync(
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        DateTimeOffset now = utcNow;

        return await dbContext.ClassGroups
            .Where(x => x.Enrolments.Any(enrolment =>
                enrolment.Status == EnrolmentStatus.PendingPayment && enrolment.PaymentDueOnUtc <= now))
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountUnpaidEnrolmentsAsync(Guid studentUserId, CancellationToken cancellationToken = default)
    {
        return await dbContext.ClassGroups
            .SelectMany(x => x.Enrolments)
            .CountAsync(
                enrolment => enrolment.StudentUserId == studentUserId &&
                             (enrolment.Status == EnrolmentStatus.Waitlisted ||
                              enrolment.Status == EnrolmentStatus.PendingPayment),
                cancellationToken);
    }

    public async Task<bool> IsCodeTakenAsync(
        string code,
        Guid? exceptClassGroupId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.ClassGroups.AnyAsync(
            x => x.Code == code && (exceptClassGroupId == null || x.Id != exceptClassGroupId),
            cancellationToken);
    }

    public async Task<bool> ExistsForCourseAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        return await dbContext.ClassGroups.AnyAsync(x => x.CourseId == courseId, cancellationToken);
    }

    public void Insert(ClassGroup classGroup)
    {
        dbContext.ClassGroups.Add(classGroup);
    }

    public void Remove(ClassGroup classGroup)
    {
        dbContext.ClassGroups.Remove(classGroup);
    }
}
