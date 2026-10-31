namespace FourPrep.Modules.Chat.Application.Abstractions.Students;

public sealed record ChatStudent(Guid UserId, string DisplayName, Guid? MentorUserId);
