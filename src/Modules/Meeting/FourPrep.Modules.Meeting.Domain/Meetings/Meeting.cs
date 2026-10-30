namespace FourPrep.Modules.Meeting.Domain.Meetings;

using FourPrep.Common.Domain;

public sealed class Meeting
{
    public const int ConsultationMinutes = 45;

    public const int MaxScheduledPerBooker = 3;

    public Guid Id { get; set; }
    public MeetingKind Kind { get; set; }
    public MeetingMode Mode { get; set; }
    public MeetingStatus Status { get; set; }
    public Guid StudentUserId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? StudentPhone { get; set; }
    public Guid BookedByUserId { get; set; }
    public Guid StaffUserId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public DateTimeOffset StartsOnUtc { get; set; }
    public int DurationMinutes { get; set; }
    public string? MeetingUrl { get; set; }
    public string? OfficeCountryCode { get; set; }
    public string? OfficeCity { get; set; }
    public string? OfficeAddress { get; set; }
    public string? StaffNotes { get; set; }
    public Guid? CancelledByUserId { get; set; }
    public string? CancellationReason { get; set; }
    public DateTimeOffset? CancelledOnUtc { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset UpdatedOnUtc { get; set; }

    public DateTimeOffset EndsOnUtc => StartsOnUtc.AddMinutes(DurationMinutes);

    public static Meeting BookConsultation(
        MeetingMode mode,
        Guid studentUserId,
        string studentName,
        string? studentPhone,
        Guid bookedByUserId,
        Guid staffUserId,
        string staffName,
        DateTime startsOnUtc,
        string? meetingUrl,
        string? officeCountryCode,
        string? officeCity,
        string? officeAddress,
        DateTime utcNow)
    {
        return new Meeting
        {
            Id = Guid.NewGuid(),
            Kind = MeetingKind.Consultation,
            Mode = mode,
            Status = MeetingStatus.Scheduled,
            StudentUserId = studentUserId,
            StudentName = studentName,
            StudentPhone = studentPhone,
            BookedByUserId = bookedByUserId,
            StaffUserId = staffUserId,
            StaffName = staffName,
            StartsOnUtc = startsOnUtc,
            DurationMinutes = ConsultationMinutes,
            MeetingUrl = meetingUrl,
            OfficeCountryCode = officeCountryCode,
            OfficeCity = officeCity,
            OfficeAddress = officeAddress,
            CreatedOnUtc = utcNow,
            UpdatedOnUtc = utcNow
        };
    }

    public bool HasStarted(DateTime utcNow) => StartsOnUtc <= utcNow;

    public Result Cancel(Guid cancelledByUserId, string? reason, DateTime utcNow)
    {
        if (Status != MeetingStatus.Scheduled)
        {
            return Result.Failure(MeetingErrors.NotScheduled);
        }

        Status = MeetingStatus.Cancelled;
        StudentPhone = null;
        CancelledByUserId = cancelledByUserId;
        CancellationReason = reason;
        CancelledOnUtc = utcNow;
        UpdatedOnUtc = utcNow;

        return Result.Success();
    }

    public Result RecordAttendance(MeetingStatus outcome, DateTime utcNow)
    {
        if (outcome is not (MeetingStatus.Held or MeetingStatus.NoShow))
        {
            return Result.Failure(MeetingErrors.InvalidAttendance);
        }

        if (Status is not (MeetingStatus.Scheduled or MeetingStatus.Held or MeetingStatus.NoShow))
        {
            return Result.Failure(MeetingErrors.NotScheduled);
        }

        if (!HasStarted(utcNow))
        {
            return Result.Failure(MeetingErrors.NotStartedYet);
        }

        Status = outcome;
        StudentPhone = null;
        UpdatedOnUtc = utcNow;

        return Result.Success();
    }

    public Result ChangeLink(string meetingUrl, DateTime utcNow)
    {
        if (Mode != MeetingMode.Online)
        {
            return Result.Failure(MeetingErrors.NotOnline);
        }

        if (Status != MeetingStatus.Scheduled)
        {
            return Result.Failure(MeetingErrors.NotScheduled);
        }

        MeetingUrl = meetingUrl;
        UpdatedOnUtc = utcNow;

        return Result.Success();
    }

    public void UpdateNotes(string? notes, DateTime utcNow)
    {
        StaffNotes = notes;
        UpdatedOnUtc = utcNow;
    }
}
