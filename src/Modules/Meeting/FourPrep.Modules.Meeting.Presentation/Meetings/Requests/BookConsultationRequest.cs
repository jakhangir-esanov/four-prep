namespace FourPrep.Modules.Meeting.Presentation.Meetings.Requests;

using FourPrep.Modules.Meeting.Domain.Meetings;

internal sealed record BookConsultationRequest
{
    public Guid StudentUserId { get; init; }
    public Guid StaffUserId { get; init; }
    public DateTime StartsOnUtc { get; init; }
    public MeetingMode Mode { get; init; }
    public string? PhoneNumber { get; init; }
}
