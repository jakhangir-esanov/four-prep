namespace FourPrep.Modules.Auth.Application.Users.BlockUser;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Modules.Auth.Domain.Users;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Auth.Domain.UserSessions;
using FourPrep.Modules.Auth.Application.Abstractions.Data;
using FourPrep.Modules.Auth.Application.Abstractions.Identity;
using FourPrep.Modules.Auth.Application.Abstractions.Authentication;

public sealed record BlockUserCommand(Guid UserId) : ICommand;

internal sealed class BlockUserCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IDateTimeProvider dateTimeProvider,
    IUserSessionRepository sessionRepository,
    IIdentityProviderClient identityProviderClient) : ICommandHandler<BlockUserCommand>
{
    public async Task<Result> Handle(BlockUserCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == currentUser.UserId)
        {
            return Result.Failure(UserErrors.CannotBlockSelf);
        }

        var user = await identityProviderClient.GetUserAsync(request.UserId, cancellationToken);
        if (user.IsFailure)
        {
            return Result.Failure(user.Error);
        }

        if (!user.Data.IsActive)
        {
            return Result.Failure(UserErrors.AlreadyBlocked);
        }

        var disabled = await identityProviderClient.SetUserEnabledAsync(request.UserId, false, cancellationToken);
        if (disabled.IsFailure)
        {
            return disabled;
        }

        var loggedOut = await identityProviderClient.LogoutUserSessionsAsync(request.UserId, cancellationToken);
        if (loggedOut.IsFailure)
        {
            return loggedOut;
        }

        await sessionRepository.RevokeAllByUserIdAsync(request.UserId, dateTimeProvider.UtcNow, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
