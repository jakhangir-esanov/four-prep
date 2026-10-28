namespace FourPrep.Modules.Student.Domain.StudentParents;

public interface IStudentParentRepository
{
    Task<StudentParent?> GetAsync(Guid studentId, Guid parentUserId, CancellationToken cancellationToken = default);

    Task<int> CountByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default);

    void Insert(StudentParent studentParent);

    void Remove(StudentParent studentParent);

    Task RemoveAllByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default);

    Task RemoveAllByParentUserIdAsync(Guid parentUserId, CancellationToken cancellationToken = default);
}
