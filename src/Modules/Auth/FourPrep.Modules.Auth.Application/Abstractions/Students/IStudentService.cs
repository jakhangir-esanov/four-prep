namespace FourPrep.Modules.Auth.Application.Abstractions.Students;

using FourPrep.Common.Domain;

public interface IStudentService
{
    Task<Result<Guid>> CreateStudentAsync(
        Guid userId,
        string userName,
        string? country,
        CancellationToken cancellationToken = default);

    Task<Result> EraseUserDataAsync(Guid userId, CancellationToken cancellationToken = default);
}
