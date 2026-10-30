namespace FourPrep.Modules.Meeting.Application.Meetings.GetMeetings;

using FourPrep.Modules.Meeting.Domain.Meetings;

public sealed record MeetingResponse
{
    public Guid Id { get; init; }
    public MeetingKind Kind { get; init; }
    public MeetingMode Mode { get; init; }
    public MeetingStatus Status { get; init; }
    public Guid StudentUserId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public string? StudentPhone { get; init; }
    public Guid? BookedByUserId { get; init; }
    public bool BookedByMe { get; init; }
    public Guid? StaffUserId { get; init; }
    public string StaffName { get; init; } = string.Empty;
    public DateTime StartsOnUtc { get; init; }
    public int DurationMinutes { get; init; }
    public string? MeetingUrl { get; init; }
    public string? OfficeCountryCode { get; init; }
    public string? OfficeCity { get; init; }
    public string? OfficeAddress { get; init; }
    public string? StaffNotes { get; init; }
    public string? CancellationReason { get; init; }
    public DateTime? CancelledOnUtc { get; init; }
    public DateTime CreatedOnUtc { get; init; }

    internal MeetingResponse ForFamily(Guid currentUserId) =>
        this with
        {
            BookedByMe = BookedByUserId == currentUserId,
            BookedByUserId = null,
            StaffUserId = null,
            StaffNotes = null,
            MeetingUrl = Status == MeetingStatus.Scheduled ? MeetingUrl : null
        };

    internal MeetingResponse ForStaff(Guid currentUserId) =>
        this with
        {
            BookedByMe = BookedByUserId == currentUserId,
            BookedByUserId = null
        };
}
