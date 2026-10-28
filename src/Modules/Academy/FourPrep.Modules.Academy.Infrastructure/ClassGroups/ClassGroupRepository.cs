namespace FourPrep.Modules.Academy.Infrastructure.ClassGroups;

using Microsoft.EntityFrameworkCore;
using FourPrep.Modules.Academy.Domain.ClassGroups;
using FourPrep.Modules.Academy.Infrastructure.Database;

internal sealed class ClassGroupRepository(AcademyDbContext dbContext) : IClassGroupRepository
{
    public async Task<ClassGroup?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.ClassGroups.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
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
