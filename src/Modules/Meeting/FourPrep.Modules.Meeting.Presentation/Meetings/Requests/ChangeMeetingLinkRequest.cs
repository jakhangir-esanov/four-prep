namespace FourPrep.Modules.Meeting.Presentation.Meetings.Requests;

internal sealed record ChangeMeetingLinkRequest
{
    public string? MeetingUrl { get; init; }
}
