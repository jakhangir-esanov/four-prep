namespace FourPrep.Modules.Auth.Application.Users.GetUser;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Auth.Application.Abstractions.Identity;

public sealed record GetUserQuery(Guid UserId) : IQuery<UserResponse>;

internal sealed class GetUserQueryHandler(IIdentityProviderClient identityProviderClient)
    : IQueryHandler<GetUserQuery, UserResponse>
{
    public Task<Result<UserResponse>> Handle(GetUserQuery request, CancellationToken cancellationToken) =>
        identityProviderClient.GetUserAsync(request.UserId, cancellationToken);
}
