namespace FourPrep.Modules.Meeting.Application.Abstractions.Students;

public sealed record MeetingStudent(Guid UserId, string DisplayName, Guid? MentorUserId);
