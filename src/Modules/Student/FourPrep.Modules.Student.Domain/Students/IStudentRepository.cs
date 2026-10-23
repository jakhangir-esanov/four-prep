namespace FourPrep.Modules.Student.Domain.Students;

public interface IStudentRepository
{
    Task<Student?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Student?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Student?> GetByLinkCodeAsync(string linkCode, CancellationToken cancellationToken = default);

    Task<bool> IsLinkCodeTakenAsync(string linkCode, CancellationToken cancellationToken = default);

    void Insert(Student student);
}
