namespace FourPrep.Modules.Auth.Application.Users.GetUserRoles;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Auth.Application.Abstractions.Identity;

public sealed record GetUserRolesQuery(Guid UserId) : IQuery<IReadOnlyList<string>>;

internal sealed class GetUserRolesQueryHandler(
    IIdentityProviderClient identityProviderClient) : IQueryHandler<GetUserRolesQuery, IReadOnlyList<string>>
{
    public Task<Result<IReadOnlyList<string>>> Handle(
        GetUserRolesQuery request,
        CancellationToken cancellationToken) =>
        identityProviderClient.GetUserRealmRolesAsync(request.UserId, cancellationToken);
}
