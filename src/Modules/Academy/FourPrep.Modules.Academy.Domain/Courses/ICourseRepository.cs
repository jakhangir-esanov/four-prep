namespace FourPrep.Modules.Academy.Domain.Courses;

public interface ICourseRepository
{
    Task<Course?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> IsSlugTakenAsync(string slug, Guid? exceptCourseId, CancellationToken cancellationToken = default);

    void Insert(Course course);

    void Remove(Course course);
}
