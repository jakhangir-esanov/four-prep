namespace FourPrep.Modules.Academy.Domain.ClassGroups;

public interface IClassGroupRepository
{
    Task<ClassGroup?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ClassGroup?> GetByEnrolmentIdAsync(Guid enrolmentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClassGroup>> GetWithPaymentsDueAsync(DateTime utcNow, CancellationToken cancellationToken = default);

    Task<int> CountUnpaidEnrolmentsAsync(Guid studentUserId, CancellationToken cancellationToken = default);

    Task<bool> IsCodeTakenAsync(string code, Guid? exceptClassGroupId, CancellationToken cancellationToken = default);

    Task<bool> ExistsForCourseAsync(Guid courseId, CancellationToken cancellationToken = default);

    void Insert(ClassGroup classGroup);

    void Remove(ClassGroup classGroup);
}
