namespace FourPrep.Modules.Meeting.Presentation.Meetings.Requests;

internal sealed record CancelMeetingRequest
{
    public string? Reason { get; init; }
}
