namespace FourPrep.Modules.Meeting.Presentation.Meetings.Requests;

using FourPrep.Modules.Meeting.Domain.Meetings;

internal sealed record GetConsultationSlotsRequest
{
    public Guid StudentUserId { get; init; }
    public MeetingMode? Mode { get; init; }
    public DateTime? FromUtc { get; init; }
    public DateTime? ToUtc { get; init; }
}
