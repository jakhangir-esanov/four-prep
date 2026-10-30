namespace FourPrep.Modules.Meeting.Application.Abstractions.Auth;

using FourPrep.Common.Domain;

public interface IAuthService
{
    Task<Result<IReadOnlyList<string>>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default);
}
