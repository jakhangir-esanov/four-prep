namespace FourPrep.Modules.Auth.Student;

using MediatR;
using FourPrep.Common.Domain;
using FourPrep.Modules.Auth.Application.Abstractions.Students;
using FourPrep.Modules.Student.Application.Students.CreateStudent;

public sealed class StudentService(ISender sender) : IStudentService
{
    public async Task<Result<Guid>> CreateStudentAsync(
        Guid userId,
        string userName,
        string? country,
        CancellationToken cancellationToken = default)
    {
        return await sender.Send(new CreateStudentCommand(userId, userName, country), cancellationToken);
    }
}
