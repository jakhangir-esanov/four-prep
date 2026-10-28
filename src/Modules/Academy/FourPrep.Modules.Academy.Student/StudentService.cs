namespace FourPrep.Modules.Academy.Student;

using MediatR;
using FourPrep.Common.Domain;
using FourPrep.Modules.Academy.Application.Abstractions.Students;
using FourPrep.Modules.Student.Application.Students.HasFamilyAccess;

public sealed class StudentService(ISender sender) : IStudentService
{
    public async Task<Result<bool>> HasFamilyAccessAsync(
        Guid userId,
        Guid studentUserId,
        CancellationToken cancellationToken = default)
    {
        return await sender.Send(new HasFamilyAccessQuery(userId, studentUserId), cancellationToken);
    }
}
