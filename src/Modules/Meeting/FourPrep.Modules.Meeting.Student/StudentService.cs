namespace FourPrep.Modules.Meeting.Student;

using MediatR;
using FourPrep.Common.Domain;
using FourPrep.Modules.Meeting.Application.Abstractions.Students;
using FourPrep.Modules.Student.Application.Students.HasFamilyAccess;
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

    public async Task<Result<MeetingStudent>> GetStudentAsync(Guid studentUserId, CancellationToken cancellationToken = default)
    {
        var contact = await sender.Send(new GetStudentContactQuery(studentUserId), cancellationToken);

        return contact.IsSuccess
            ? new MeetingStudent(contact.Data.UserId, contact.Data.DisplayName, contact.Data.MentorUserId)
            : Result.Failure<MeetingStudent>(contact.Error);
    }
}
