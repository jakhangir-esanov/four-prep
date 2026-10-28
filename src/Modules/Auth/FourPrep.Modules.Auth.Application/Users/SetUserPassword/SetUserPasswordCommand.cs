namespace FourPrep.Modules.Auth.Application.Users.SetUserPassword;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Auth.Domain.UserSessions;
using FourPrep.Modules.Auth.Application.Abstractions.Data;
using FourPrep.Modules.Auth.Application.Abstractions.Identity;

public sealed record SetUserPasswordCommand(Guid UserId, string Password) : ICommand;

internal sealed class SetUserPasswordCommandHandler(
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IUserSessionRepository sessionRepository,
    IIdentityProviderClient identityProviderClient) : ICommandHandler<SetUserPasswordCommand>
{
    public async Task<Result> Handle(SetUserPasswordCommand request, CancellationToken cancellationToken)
    {
        var changed = await identityProviderClient.SetUserPasswordAsync(request.UserId, request.Password, cancellationToken);
        if (changed.IsFailure)
        {
            return changed;
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
