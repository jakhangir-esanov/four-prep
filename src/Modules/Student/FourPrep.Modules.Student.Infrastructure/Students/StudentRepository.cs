namespace FourPrep.Modules.Student.Infrastructure.Students;

using Microsoft.EntityFrameworkCore;
using FourPrep.Modules.Student.Domain.Students;
using FourPrep.Modules.Student.Infrastructure.Database;

internal sealed class StudentRepository(StudentDbContext dbContext) : IStudentRepository
{
    public async Task<Student?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Students.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Student?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Students.SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public async Task<Student?> GetByLinkCodeAsync(string linkCode, CancellationToken cancellationToken = default)
    {
        return await dbContext.Students.SingleOrDefaultAsync(x => x.LinkCode == linkCode, cancellationToken);
    }

    public async Task<bool> IsLinkCodeTakenAsync(string linkCode, CancellationToken cancellationToken = default)
    {
        return await dbContext.Students.AnyAsync(x => x.LinkCode == linkCode, cancellationToken);
    }

    public void Insert(Student student)
    {
        dbContext.Students.Add(student);
    }
}
