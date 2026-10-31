namespace FourPrep.Modules.Chat.Application.Abstractions.Students;

using FourPrep.Common.Domain;

public interface IStudentService
{
    Task<Result<bool>> HasFamilyAccessAsync(Guid userId, Guid studentUserId, CancellationToken cancellationToken = default);

    Task<Result<ChatStudent>> GetStudentAsync(Guid studentUserId, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<Guid>>> GetMenteeUserIdsAsync(Guid mentorUserId, CancellationToken cancellationToken = default);
}
