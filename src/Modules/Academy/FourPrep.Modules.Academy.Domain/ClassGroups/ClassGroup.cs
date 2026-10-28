namespace FourPrep.Modules.Academy.Domain.ClassGroups;

using FourPrep.Common.Domain;
using FourPrep.Modules.Academy.Domain.ScheduleSlots;

public sealed class ClassGroup
{
    private static readonly Dictionary<ClassGroupStatus, ClassGroupStatus[]> AllowedStatusChanges = new()
    {
        [ClassGroupStatus.Draft] = [ClassGroupStatus.Open, ClassGroupStatus.Cancelled],
        [ClassGroupStatus.Open] = [ClassGroupStatus.Closed, ClassGroupStatus.Cancelled],
        [ClassGroupStatus.Closed] = [ClassGroupStatus.Open, ClassGroupStatus.Cancelled],
        [ClassGroupStatus.Cancelled] = []
    };

    private readonly List<ScheduleSlot> _schedule = [];

    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string Code { get; set; } = string.Empty;
    public ClassMode Mode { get; set; }
    public string? CountryCode { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public Dictionary<string, string> EntryLevel { get; set; } = [];
    public string TimeZone { get; set; } = string.Empty;
    public DateOnly StartsOn { get; set; }
    public int Weeks { get; set; }
    public int Capacity { get; set; }
    public ClassGroupStatus Status { get; set; }
    public Guid? TeacherUserId { get; set; }
    public string? TeacherName { get; set; }
    public IReadOnlyCollection<ScheduleSlot> Schedule => _schedule;
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset UpdatedOnUtc { get; set; }

    public bool IsCancelled => Status == ClassGroupStatus.Cancelled;

    public static ClassGroup Create(
        Guid courseId,
        string code,
        ClassMode mode,
        string? countryCode,
        string? city,
        string? address,
        Dictionary<string, string> entryLevel,
        string timeZone,
        DateOnly startsOn,
        int weeks,
        int capacity,
        IEnumerable<ScheduleSlot> schedule,
        DateTime utcNow)
    {
        var classGroup = new ClassGroup
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Status = ClassGroupStatus.Draft,
            CreatedOnUtc = utcNow
        };

        classGroup.Update(
            code,
            mode,
            countryCode,
            city,
            address,
            entryLevel,
            timeZone,
            startsOn,
            weeks,
            capacity,
            schedule,
            utcNow);

        return classGroup;
    }

    public void Update(
        string code,
        ClassMode mode,
        string? countryCode,
        string? city,
        string? address,
        Dictionary<string, string> entryLevel,
        string timeZone,
        DateOnly startsOn,
        int weeks,
        int capacity,
        IEnumerable<ScheduleSlot> schedule,
        DateTime utcNow)
    {
        Code = code;
        Mode = mode;
        CountryCode = countryCode;
        City = city;
        Address = address;
        EntryLevel = entryLevel;
        TimeZone = timeZone;
        StartsOn = startsOn;
        Weeks = weeks;
        Capacity = capacity;
        _schedule.Clear();
        _schedule.AddRange(schedule);
        UpdatedOnUtc = utcNow;
    }

    public Result ChangeStatus(ClassGroupStatus status, DateTime utcNow)
    {
        if (!AllowedStatusChanges[Status].Contains(status))
        {
            return Result.Failure(ClassGroupErrors.InvalidStatusChange);
        }

        Status = status;
        UpdatedOnUtc = utcNow;

        return Result.Success();
    }

    public void AssignTeacher(Guid teacherUserId, string teacherName, DateTime utcNow)
    {
        TeacherUserId = teacherUserId;
        TeacherName = teacherName;
        UpdatedOnUtc = utcNow;
    }

    public void RemoveTeacher(DateTime utcNow)
    {
        TeacherUserId = null;
        TeacherName = null;
        UpdatedOnUtc = utcNow;
    }
}
