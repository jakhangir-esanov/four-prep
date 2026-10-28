namespace FourPrep.Modules.Academy.Domain.ClassGroups;

using FourPrep.Common.Domain;
using FourPrep.Modules.Academy.Domain.Enrolments;
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
    private readonly List<Enrolment> _enrolments = [];

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
    public IReadOnlyCollection<Enrolment> Enrolments => _enrolments;
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset UpdatedOnUtc { get; set; }

    public bool IsCancelled => Status == ClassGroupStatus.Cancelled;

    public int SeatsTaken => _enrolments.Count(enrolment => enrolment.HoldsSeat);

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
        FillFreeSeats(utcNow);
    }

    public Result ChangeStatus(ClassGroupStatus status, DateTime utcNow)
    {
        if (!AllowedStatusChanges[Status].Contains(status))
        {
            return Result.Failure(ClassGroupErrors.InvalidStatusChange);
        }

        Status = status;
        UpdatedOnUtc = utcNow;

        if (IsCancelled)
        {
            EndCurrentEnrolments(utcNow);
        }

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

    public Result<Enrolment> Enrol(Guid studentUserId, Guid enrolledByUserId, DateTime utcNow)
    {
        if (Status != ClassGroupStatus.Open)
        {
            return Result.Failure<Enrolment>(ClassGroupErrors.NotOpen);
        }

        if (_enrolments.Any(enrolment => enrolment.StudentUserId == studentUserId && enrolment.IsCurrent))
        {
            return Result.Failure<Enrolment>(EnrolmentErrors.AlreadyEnrolled);
        }

        var seatAvailable = SeatsTaken < Capacity && !Waitlist().Any();
        var enrolment = Enrolment.Create(studentUserId, enrolledByUserId, seatAvailable, utcNow);

        _enrolments.Add(enrolment);
        UpdatedOnUtc = utcNow;

        return enrolment;
    }

    public Result CancelEnrolment(Guid enrolmentId, DateTime utcNow) =>
        ChangeEnrolment(enrolmentId, enrolment => enrolment.Cancel(utcNow), utcNow);

    public Result RemoveEnrolment(Guid enrolmentId, DateTime utcNow) =>
        ChangeEnrolment(enrolmentId, enrolment => enrolment.Remove(utcNow), utcNow);

    public Result ActivateEnrolment(Guid enrolmentId, DateTime utcNow) =>
        ChangeEnrolment(enrolmentId, enrolment => enrolment.Activate(utcNow), utcNow);

    public int ExpireUnpaidEnrolments(DateTime utcNow)
    {
        var overdue = _enrolments.Where(enrolment => enrolment.IsPaymentOverdue(utcNow)).ToList();
        if (overdue.Count == 0)
        {
            return 0;
        }

        foreach (var enrolment in overdue)
        {
            enrolment.Expire(utcNow);
        }

        UpdatedOnUtc = utcNow;
        FillFreeSeats(utcNow);

        return overdue.Count;
    }

    private Result ChangeEnrolment(Guid enrolmentId, Func<Enrolment, Result> change, DateTime utcNow)
    {
        var enrolment = _enrolments.SingleOrDefault(x => x.Id == enrolmentId);
        if (enrolment is null)
        {
            return Result.Failure(EnrolmentErrors.NotFound);
        }

        var changed = change(enrolment);
        if (changed.IsFailure)
        {
            return changed;
        }

        UpdatedOnUtc = utcNow;
        FillFreeSeats(utcNow);

        return Result.Success();
    }

    private void EndCurrentEnrolments(DateTime utcNow)
    {
        foreach (var enrolment in _enrolments.Where(enrolment => enrolment.IsCurrent))
        {
            enrolment.EndWithGroup(utcNow);
        }
    }

    private void FillFreeSeats(DateTime utcNow)
    {
        if (IsCancelled)
        {
            return;
        }

        foreach (var next in Waitlist().Take(Math.Max(0, Capacity - SeatsTaken)).ToList())
        {
            next.OfferSeat(utcNow);
        }
    }

    private IEnumerable<Enrolment> Waitlist() =>
        _enrolments
            .Where(enrolment => enrolment.Status == EnrolmentStatus.Waitlisted)
            .OrderBy(enrolment => enrolment.CreatedOnUtc)
            .ThenBy(enrolment => enrolment.Id);
}
