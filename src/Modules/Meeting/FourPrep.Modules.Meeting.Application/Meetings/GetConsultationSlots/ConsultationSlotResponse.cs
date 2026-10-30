namespace FourPrep.Modules.Meeting.Application.Meetings.GetConsultationSlots;

using FourPrep.Modules.Meeting.Domain.Meetings;

public sealed record ConsultationSlotResponse(
    Guid StaffUserId,
    string StaffName,
    DateTime StartsOnUtc,
    int DurationMinutes,
    IReadOnlyList<MeetingMode> Modes);
