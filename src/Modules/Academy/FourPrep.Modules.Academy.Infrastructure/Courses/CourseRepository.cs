namespace FourPrep.Modules.Academy.Infrastructure.Courses;

using Microsoft.EntityFrameworkCore;
using FourPrep.Modules.Academy.Domain.Courses;
using FourPrep.Modules.Academy.Infrastructure.Database;

internal sealed class CourseRepository(AcademyDbContext dbContext) : ICourseRepository
{
    public async Task<Course?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Courses.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Courses.AnyAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> IsSlugTakenAsync(
        string slug,
        Guid? exceptCourseId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Courses.AnyAsync(
            x => x.Slug == slug && (exceptCourseId == null || x.Id != exceptCourseId),
            cancellationToken);
    }

    public void Insert(Course course)
    {
        dbContext.Courses.Add(course);
    }

    public void Remove(Course course)
    {
        dbContext.Courses.Remove(course);
    }
}
