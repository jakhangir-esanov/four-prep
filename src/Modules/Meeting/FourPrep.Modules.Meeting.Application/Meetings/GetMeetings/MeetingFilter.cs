namespace FourPrep.Modules.Meeting.Application.Meetings.GetMeetings;

using FourPrep.Common.Application.Data;
using FourPrep.Modules.Meeting.Domain.Meetings;

public sealed record MeetingFilter(
    MeetingStatus? Status = null,
    MeetingMode? Mode = null,
    Guid? StaffUserId = null,
    Guid? StudentUserId = null,
    DateTime? FromUtc = null,
    DateTime? ToUtc = null,
    string? Search = null)
{
    public static readonly MeetingFilter None = new();

    public string? SearchPattern => LikePatterns.Contains(Search);
}
