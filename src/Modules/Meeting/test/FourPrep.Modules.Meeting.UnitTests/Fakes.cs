namespace FourPrep.Modules.Meeting.UnitTests;

using System.Data.Common;
using FourPrep.Common.Domain;
using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Domain.Authorization;
using FourPrep.Modules.Meeting.Domain.Meetings;
using FourPrep.Modules.Meeting.Domain.StaffSchedules;
using FourPrep.Modules.Meeting.Application.StaffSchedules;
using FourPrep.Modules.Meeting.Application.Abstractions.Auth;
using FourPrep.Modules.Meeting.Application.Abstractions.Data;
using FourPrep.Modules.Meeting.Application.Abstractions.Students;
using FourPrep.Modules.Meeting.Application.Abstractions.Authentication;

internal sealed class FakeClock : IDateTimeProvider
{
    public DateTime UtcNow { get; set; } = new(2026, 10, 30, 9, 0, 0, DateTimeKind.Utc);
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

internal sealed class FakeStaffScheduleRepository : IStaffScheduleRepository
{
    public List<StaffSchedule> Schedules { get; } = [];

    public StaffSchedule Add(Guid staffUserId, StaffScheduleDetails? details = null)
    {
        var schedule = (details ?? ScheduleSamples.NewYork()).ToSchedule(staffUserId, new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc));
        Schedules.Add(schedule);
        return schedule;
    }

    public Task<StaffSchedule?> GetByStaffUserIdAsync(Guid staffUserId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Schedules.SingleOrDefault(x => x.StaffUserId == staffUserId));

    public void Insert(StaffSchedule schedule) => Schedules.Add(schedule);
}

internal sealed class FakeMeetingRepository : IMeetingRepository
{
    public List<Meeting> Meetings { get; } = [];

    public Meeting Add(
        Guid studentUserId,
        Guid staffUserId,
        DateTime startsOnUtc,
        MeetingMode mode = MeetingMode.Online,
        Guid? bookedByUserId = null)
    {
        var meeting = Meeting.BookConsultation(
            mode,
            studentUserId,
            "Aziza Karimova",
            mode == MeetingMode.Phone ? "+14155550123" : null,
            bookedByUserId ?? studentUserId,
            staffUserId,
            "Ms Rivera",
            startsOnUtc,
            mode == MeetingMode.Online ? "https://zoom.us/j/1234567890" : null,
            null,
            null,
            null,
            new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc));
        Meetings.Add(meeting);
        return meeting;
    }

    public Task<Meeting?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Meetings.SingleOrDefault(x => x.Id == id));

    public Task<bool> HasStaffOverlapAsync(
        Guid staffUserId,
        DateTime startsOnUtc,
        DateTime endsOnUtc,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Meetings.Any(x =>
            x.StaffUserId == staffUserId &&
            x.Status == MeetingStatus.Scheduled &&
            x.StartsOnUtc < endsOnUtc &&
            startsOnUtc < x.EndsOnUtc));

    public Task<int> CountScheduledBookedByAsync(Guid bookedByUserId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Meetings.Count(x => x.BookedByUserId == bookedByUserId && x.Status == MeetingStatus.Scheduled));

    public Task<bool> HasScheduledConsultationAsync(Guid studentUserId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Meetings.Any(x =>
            x.StudentUserId == studentUserId &&
            x.Kind == MeetingKind.Consultation &&
            x.Status == MeetingStatus.Scheduled));

    public void Insert(Meeting meeting) => Meetings.Add(meeting);
}

internal sealed class FakeStudentService : IStudentService
{
    public HashSet<(Guid UserId, Guid StudentUserId)> Access { get; } = [];

    public Dictionary<Guid, MeetingStudent> Students { get; } = [];

    public void AddFamily(Guid userId, Guid studentUserId, Guid? mentorUserId = null)
    {
        Access.Add((userId, studentUserId));
        Access.Add((studentUserId, studentUserId));
        Students[studentUserId] = new MeetingStudent(studentUserId, "Aziza Karimova", mentorUserId);
    }

    public Task<Result<bool>> HasFamilyAccessAsync(Guid userId, Guid studentUserId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success(Access.Contains((userId, studentUserId))));

    public Task<Result<MeetingStudent>> GetStudentAsync(Guid studentUserId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Students.TryGetValue(studentUserId, out var student)
            ? Result.Success(student)
            : Result.Failure<MeetingStudent>(Error.NotFound("Student.NotFound", "Not found.", "Topilmadi.", "Не найден.")));
}

internal sealed class FakeAuthService : IAuthService
{
    public Dictionary<Guid, IReadOnlyList<string>> Roles { get; } = [];

    public void AddMentor(Guid userId) => Roles[userId] = [FourPrep.Common.Domain.Authorization.Roles.Mentor];

    public Task<Result<IReadOnlyList<string>>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success(Roles.TryGetValue(userId, out var roles) ? roles : []));
}

internal sealed class UnusedConnectionFactory : IDbConnectionFactory
{
    public ValueTask<DbConnection> OpenConnectionAsync() =>
        throw new InvalidOperationException("The handler should not reach the database.");
}

internal static class ScheduleSamples
{
    public static readonly DateTime MondayNineInNewYorkUtc = new(2026, 11, 2, 14, 0, 0, DateTimeKind.Utc);

    public static StaffScheduleDetails NewYork() =>
        new(
            " Ms Rivera ",
            "America/New_York",
            " https://zoom.us/j/1234567890 ",
            "us",
            " New York ",
            " 1 Example Avenue ",
            [
                new AvailabilityWindowInput(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(11, 0), MeetingMode.Online),
                new AvailabilityWindowInput(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(10, 0), MeetingMode.Phone),
                new AvailabilityWindowInput(DayOfWeek.Tuesday, new TimeOnly(14, 0), new TimeOnly(16, 0), MeetingMode.Office)
            ]);
}
