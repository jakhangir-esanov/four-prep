namespace FourPrep.Modules.Academy.UnitTests;

using System.Data.Common;
using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Modules.Academy.Domain.Courses;
using FourPrep.Modules.Academy.Domain.ClassGroups;
using FourPrep.Modules.Academy.Application.ClassGroups;
using FourPrep.Modules.Academy.Application.Abstractions.Auth;
using FourPrep.Modules.Academy.Application.Abstractions.Data;

internal sealed class FakeClock : IDateTimeProvider
{
    public DateTime UtcNow { get; } = new(2026, 10, 29, 9, 0, 0, DateTimeKind.Utc);
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveCount++;
        return Task.FromResult(0);
    }

    public Task<DbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();
}

internal sealed class FakeCourseRepository : ICourseRepository
{
    public List<Course> Courses { get; } = [];

    public Course Add(string slug = "sat")
    {
        var course = Course.Create(
            slug,
            new Dictionary<string, string> { ["en"] = "SAT" },
            [],
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        Courses.Add(course);
        return course;
    }

    public Task<Course?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Courses.SingleOrDefault(x => x.Id == id));

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Courses.Any(x => x.Id == id));

    public Task<bool> IsSlugTakenAsync(string slug, Guid? exceptCourseId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Courses.Any(x => x.Slug == slug && x.Id != exceptCourseId));

    public void Insert(Course course) => Courses.Add(course);

    public void Remove(Course course) => Courses.Remove(course);
}

internal sealed class FakeClassGroupRepository : IClassGroupRepository
{
    public List<ClassGroup> ClassGroups { get; } = [];

    public ClassGroup Add(Guid courseId, string code = "SAT-2026-OCT", ClassGroupStatus status = ClassGroupStatus.Draft)
    {
        var classGroup = ClassGroupDetailsSamples.Online() with { Code = code };
        var created = classGroup.ToClassGroup(courseId, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        created.Status = status;
        ClassGroups.Add(created);
        return created;
    }

    public Task<ClassGroup?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(ClassGroups.SingleOrDefault(x => x.Id == id));

    public Task<bool> IsCodeTakenAsync(string code, Guid? exceptClassGroupId, CancellationToken cancellationToken = default) =>
        Task.FromResult(ClassGroups.Any(x => x.Code == code && x.Id != exceptClassGroupId));

    public Task<bool> ExistsForCourseAsync(Guid courseId, CancellationToken cancellationToken = default) =>
        Task.FromResult(ClassGroups.Any(x => x.CourseId == courseId));

    public void Insert(ClassGroup classGroup) => ClassGroups.Add(classGroup);

    public void Remove(ClassGroup classGroup) => ClassGroups.Remove(classGroup);
}

internal sealed class FakeAuthService : IAuthService
{
    public Dictionary<Guid, IReadOnlyList<string>> Roles { get; } = [];

    public Error? Failure { get; init; }

    public Task<Result<IReadOnlyList<string>>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (Failure is not null)
        {
            return Task.FromResult(Result.Failure<IReadOnlyList<string>>(Failure));
        }

        return Task.FromResult(Result.Success(Roles.TryGetValue(userId, out var roles) ? roles : []));
    }
}

internal static class ClassGroupDetailsSamples
{
    public static ClassGroupDetails Online() =>
        new(
            "sat-2026-oct",
            ClassMode.Online,
            null,
            null,
            null,
            new Dictionary<string, string> { ["en"] = "Algebra basics or a placement test" },
            "America/New_York",
            new DateOnly(2026, 11, 2),
            12,
            10,
            [
                new ScheduleSlotInput(DayOfWeek.Thursday, new TimeOnly(18, 0), 90),
                new ScheduleSlotInput(DayOfWeek.Monday, new TimeOnly(18, 0), 90)
            ]);

    public static ClassGroupDetails Office() =>
        Online() with
        {
            Mode = ClassMode.Office,
            CountryCode = "gb",
            City = " London ",
            Address = " 1 Example Street ",
            TimeZone = "Europe/London"
        };
}
