namespace FourPrep.Modules.Admission.UnitTests;

using System.Data.Common;
using FourPrep.Common.Domain;
using FourPrep.Common.Domain.Enums;
using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Clock;
using FourPrep.Modules.Admission.Domain.ReportedScores;
using FourPrep.Modules.Admission.Domain.AcademyApplications;
using FourPrep.Modules.Admission.Application.Abstractions.Data;
using FourPrep.Modules.Admission.Application.Abstractions.Students;
using FourPrep.Modules.Admission.Application.Abstractions.Authentication;

internal sealed class FakeClock : IDateTimeProvider
{
    public DateTime UtcNow { get; } = new(2026, 10, 31, 9, 0, 0, DateTimeKind.Utc);
}

internal sealed class FakeTimeZoneContext : ITimeZoneContext
{
    public TimeZoneInfo TimeZone => TimeZoneInfo.Utc;

    public DateOnly Today() => new(2026, 10, 31);

    public DateTime ToLocal(DateTime utc) => utc;
}

internal sealed class FakeCurrentUser(Guid userId) : ICurrentUser
{
    public Guid UserId { get; } = userId;
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

internal sealed class FakeAcademyApplicationRepository : IAcademyApplicationRepository
{
    public List<AcademyApplication> Applications { get; } = [];

    public AcademyApplication Add(Guid studentUserId)
    {
        var application = AcademyApplication.Create(
            studentUserId,
            "Aziza Karimova",
            [ApplicationInterest.AdmissionPackage],
            [ReportedScore.Create(Exam.Ielts, 6.5m)],
            [ContactChannel.Email],
            null,
            null,
            "en",
            null,
            true,
            new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc));
        Applications.Add(application);
        return application;
    }

    public Task<AcademyApplication?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Applications.SingleOrDefault(x => x.Id == id));

    public Task<bool> HasOpenAsync(Guid studentUserId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Applications.Any(x => x.StudentUserId == studentUserId && x.IsOpen));

    public void Insert(AcademyApplication application) => Applications.Add(application);
}

internal sealed class FakeStudentService : IStudentService
{
    public static readonly Error NotFound = Error.NotFound("Student.NotFound", "Not found.", "Topilmadi.", "Не найден.");

    public HashSet<(Guid UserId, Guid StudentUserId)> Access { get; } = [];

    public Dictionary<Guid, AdmissionStudent> Students { get; } = [];

    public void AddStudent(Guid studentUserId, DateOnly? dateOfBirth, Guid? parentUserId = null)
    {
        Access.Add((studentUserId, studentUserId));
        if (parentUserId is { } parent)
        {
            Access.Add((parent, studentUserId));
        }

        Students[studentUserId] = new AdmissionStudent(studentUserId, "Aziza Karimova", dateOfBirth);
    }

    public Error? AccessError { get; init; }

    public Task<Result<bool>> HasFamilyAccessAsync(Guid userId, Guid studentUserId, CancellationToken cancellationToken = default) =>
        Task.FromResult(AccessError is { } error
            ? Result.Failure<bool>(error)
            : Result.Success(Access.Contains((userId, studentUserId))));

    public Task<Result<AdmissionStudent>> GetStudentAsync(Guid studentUserId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Students.TryGetValue(studentUserId, out var student)
            ? Result.Success(student)
            : Result.Failure<AdmissionStudent>(NotFound));
}

internal sealed class UnusedConnectionFactory : IDbConnectionFactory
{
    public ValueTask<DbConnection> OpenConnectionAsync() =>
        throw new InvalidOperationException("The handler should not reach the database.");
}
