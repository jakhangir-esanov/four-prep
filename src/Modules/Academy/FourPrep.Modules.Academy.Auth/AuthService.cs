namespace FourPrep.Modules.Academy.Auth;

using MediatR;
using FourPrep.Common.Domain;
using FourPrep.Modules.Academy.Application.Abstractions.Auth;
using FourPrep.Modules.Auth.Application.Users.GetUserRoles;

public sealed class AuthService(ISender sender) : IAuthService
{
    public async Task<Result<IReadOnlyList<string>>> GetUserRolesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await sender.Send(new GetUserRolesQuery(userId), cancellationToken);
    }
}
