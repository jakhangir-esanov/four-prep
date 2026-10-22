namespace FourPrep.Modules.Auth.Application.Users.UnblockUser;

using FourPrep.Common.Domain;
using FourPrep.Modules.Auth.Domain.Users;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Auth.Application.Abstractions.Identity;

public sealed record UnblockUserCommand(Guid UserId) : ICommand;

internal sealed class UnblockUserCommandHandler(
    IIdentityProviderClient identityProviderClient) : ICommandHandler<UnblockUserCommand>
{
    public async Task<Result> Handle(UnblockUserCommand request, CancellationToken cancellationToken)
    {
        var user = await identityProviderClient.GetUserAsync(request.UserId, cancellationToken);
        if (user.IsFailure)
        {
            return Result.Failure(user.Error);
        }

        if (user.Data.IsActive)
        {
            return Result.Failure(UserErrors.NotBlocked);
        }

        return await identityProviderClient.SetUserEnabledAsync(request.UserId, true, cancellationToken);
    }
}
