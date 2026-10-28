namespace FourPrep.Modules.Student.Infrastructure.StudentParents;

using Microsoft.EntityFrameworkCore;
using FourPrep.Modules.Student.Domain.StudentParents;
using FourPrep.Modules.Student.Infrastructure.Database;

internal sealed class StudentParentRepository(StudentDbContext dbContext) : IStudentParentRepository
{
    public async Task<StudentParent?> GetAsync(
        Guid studentId,
        Guid parentUserId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.StudentParents.SingleOrDefaultAsync(
            x => x.StudentId == studentId && x.ParentUserId == parentUserId,
            cancellationToken);
    }

    public async Task<int> CountByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        return await dbContext.StudentParents.CountAsync(x => x.StudentId == studentId, cancellationToken);
    }

    public void Insert(StudentParent studentParent)
    {
        dbContext.StudentParents.Add(studentParent);
    }

    public void Remove(StudentParent studentParent)
    {
        dbContext.StudentParents.Remove(studentParent);
    }

    public async Task RemoveAllByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        dbContext.StudentParents.RemoveRange(
            await dbContext.StudentParents.Where(x => x.StudentId == studentId).ToListAsync(cancellationToken));
    }

    public async Task RemoveAllByParentUserIdAsync(Guid parentUserId, CancellationToken cancellationToken = default)
    {
        dbContext.StudentParents.RemoveRange(
            await dbContext.StudentParents.Where(x => x.ParentUserId == parentUserId).ToListAsync(cancellationToken));
    }
}
