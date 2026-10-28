namespace FourPrep.Modules.Academy.Application.Abstractions.Students;

using FourPrep.Common.Domain;

public interface IStudentService
{
    Task<Result<bool>> HasFamilyAccessAsync(Guid userId, Guid studentUserId, CancellationToken cancellationToken = default);
}
