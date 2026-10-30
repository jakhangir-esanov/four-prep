namespace FourPrep.Modules.Meeting.Presentation.Meetings.Requests;

using FourPrep.Modules.Meeting.Domain.Meetings;
using FourPrep.Modules.Meeting.Application.Meetings.GetMeetings;

internal sealed record GetMeetingsRequest
{
    public int? First { get; init; }
    public int? Rows { get; init; }
    public string? SortField { get; init; }
    public int? SortOrder { get; init; }

    public MeetingStatus? Status { get; init; }
    public MeetingMode? Mode { get; init; }
    public Guid? StaffUserId { get; init; }
    public Guid? StudentUserId { get; init; }
    public DateTime? FromUtc { get; init; }
    public DateTime? ToUtc { get; init; }
    public string? Search { get; init; }

    public MeetingFilter ToFilter() => new(Status, Mode, StaffUserId, StudentUserId, FromUtc, ToUtc, Search);
}
