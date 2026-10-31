namespace FourPrep.Modules.Chat.Application.ChatThreads;

using FourPrep.Common.Domain;
using FourPrep.Modules.Chat.Domain.ChatThreads;
using FourPrep.Modules.Chat.Application.Abstractions.Students;
using FourPrep.Modules.Chat.Application.Abstractions.Authentication;

internal static class ChatAccess
{
    public static async Task<Result<ChatStudent>> CheckAsync(
        IStudentService studentService,
        ICurrentUser currentUser,
        Guid studentUserId,
        ChatThreadKind kind,
        ChatAudience audience,
        CancellationToken cancellationToken)
    {
        if (audience == ChatAudience.Family)
        {
            var access = await studentService.HasFamilyAccessAsync(currentUser.UserId, studentUserId, cancellationToken);
            if (access.IsFailure)
            {
                return Result.Failure<ChatStudent>(access.Error);
            }

            if (!access.Data)
            {
                return Result.Failure<ChatStudent>(ChatThreadErrors.NotYourStudent);
            }
        }

        var student = await studentService.GetStudentAsync(studentUserId, cancellationToken);
        if (audience != ChatAudience.Mentor)
        {
            return student;
        }

        return student.IsSuccess && IsMentorOf(student.Data, kind, currentUser.UserId)
            ? student
            : Result.Failure<ChatStudent>(ChatThreadErrors.NotYourStudent);
    }

    private static bool IsMentorOf(ChatStudent student, ChatThreadKind kind, Guid userId) =>
        kind == ChatThreadKind.Mentor && student.MentorUserId == userId;
}
