namespace FourPrep.Modules.Meeting.Presentation.Meetings.Requests;

internal sealed record UpdateMeetingNotesRequest
{
    public string? Notes { get; init; }
}
