namespace FourPrep.Modules.Auth.Application.Account.GetMyAccount;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Auth.Application.Users.GetUser;
using FourPrep.Modules.Auth.Application.Abstractions.Identity;
using FourPrep.Modules.Auth.Application.Abstractions.Authentication;

public sealed record GetMyAccountQuery : IQuery<UserResponse>;

internal sealed class GetMyAccountQueryHandler(
    ICurrentUser currentUser,
    IIdentityProviderClient identityProviderClient) : IQueryHandler<GetMyAccountQuery, UserResponse>
{
    public Task<Result<UserResponse>> Handle(GetMyAccountQuery request, CancellationToken cancellationToken) =>
        identityProviderClient.GetUserAsync(currentUser.UserId, cancellationToken);
}
