namespace FourPrep.Modules.Chat.Student;

using MediatR;
using FourPrep.Common.Domain;
using FourPrep.Modules.Chat.Application.Abstractions.Students;
using FourPrep.Modules.Student.Application.Students.HasFamilyAccess;
using FourPrep.Modules.Student.Application.Students.GetMenteeUserIds;
using FourPrep.Modules.Student.Application.Students.GetStudentContact;

public sealed class StudentService(ISender sender) : IStudentService
{
    public async Task<Result<bool>> HasFamilyAccessAsync(
        Guid userId,
        Guid studentUserId,
        CancellationToken cancellationToken = default)
    {
        return await sender.Send(new HasFamilyAccessQuery(userId, studentUserId), cancellationToken);
    }

    public async Task<Result<ChatStudent>> GetStudentAsync(Guid studentUserId, CancellationToken cancellationToken = default)
    {
        var contact = await sender.Send(new GetStudentContactQuery(studentUserId), cancellationToken);

        return contact.IsSuccess
            ? new ChatStudent(contact.Data.UserId, contact.Data.DisplayName, contact.Data.MentorUserId)
            : Result.Failure<ChatStudent>(contact.Error);
    }

    public async Task<Result<IReadOnlyList<Guid>>> GetMenteeUserIdsAsync(
        Guid mentorUserId,
        CancellationToken cancellationToken = default)
    {
        return await sender.Send(new GetMenteeUserIdsQuery(mentorUserId), cancellationToken);
    }
}
