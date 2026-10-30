namespace FourPrep.Modules.Meeting.Presentation.Meetings.Requests;

using FourPrep.Modules.Meeting.Domain.Meetings;

internal sealed record RecordAttendanceRequest
{
    public MeetingStatus Status { get; init; }
}
