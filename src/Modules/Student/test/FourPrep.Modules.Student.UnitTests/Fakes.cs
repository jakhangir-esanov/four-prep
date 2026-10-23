namespace FourPrep.Modules.Student.UnitTests;

using System.Data.Common;
using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Modules.Student.Domain.Students;
using FourPrep.Modules.Student.Domain.StudentParents;
using FourPrep.Modules.Student.Application.Abstractions.Auth;
using FourPrep.Modules.Student.Application.Abstractions.Data;
using FourPrep.Modules.Student.Application.Abstractions.LinkCodes;
using FourPrep.Modules.Student.Application.Abstractions.Authentication;

internal sealed class FakeClock : IDateTimeProvider
{
    public DateTime UtcNow { get; } = new(2026, 10, 23, 9, 0, 0, DateTimeKind.Utc);
}

internal sealed class FakeTimeZoneContext : ITimeZoneContext
{
    public TimeZoneInfo TimeZone => TimeZoneInfo.Utc;

    public DateOnly Today() => new(2026, 10, 23);

    public DateTime ToLocal(DateTime utc) => utc;
}

internal sealed class FakeCurrentUser(Guid userId, string userName = "current.user") : ICurrentUser
{
    public Guid UserId { get; } = userId;

    public string UserName { get; } = userName;
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

internal sealed class FakeLinkCodeGenerator(params string[] codes) : ILinkCodeGenerator
{
    private readonly Queue<string> _codes = new(codes);

    public string Next() => _codes.Dequeue();
}

internal sealed class FakeStudentRepository : IStudentRepository
{
    public List<Student> Students { get; } = [];

    public Student Add(Guid userId, string linkCode = "ABCD2345")
    {
        var student = Student.Create(userId, "student", linkCode, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        Students.Add(student);
        return student;
    }

    public Task<Student?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Students.SingleOrDefault(x => x.Id == id));

    public Task<Student?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Students.SingleOrDefault(x => x.UserId == userId));

    public Task<Student?> GetByLinkCodeAsync(string linkCode, CancellationToken cancellationToken = default) =>
        Task.FromResult(Students.SingleOrDefault(x => x.LinkCode == linkCode));

    public Task<bool> IsLinkCodeTakenAsync(string linkCode, CancellationToken cancellationToken = default) =>
        Task.FromResult(Students.Any(x => x.LinkCode == linkCode));

    public void Insert(Student student) => Students.Add(student);
}

internal sealed class FakeStudentParentRepository : IStudentParentRepository
{
    public List<StudentParent> Links { get; } = [];

    public Task<StudentParent?> GetAsync(Guid studentId, Guid parentUserId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Links.SingleOrDefault(x => x.StudentId == studentId && x.ParentUserId == parentUserId));

    public Task<int> CountByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Links.Count(x => x.StudentId == studentId));

    public void Insert(StudentParent studentParent) => Links.Add(studentParent);

    public void Remove(StudentParent studentParent) => Links.Remove(studentParent);
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
