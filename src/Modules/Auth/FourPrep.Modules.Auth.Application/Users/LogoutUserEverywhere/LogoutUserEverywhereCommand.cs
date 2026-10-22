namespace FourPrep.Modules.Auth.Application.Users.LogoutUserEverywhere;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Auth.Domain.UserSessions;
using FourPrep.Modules.Auth.Application.Abstractions.Data;
using FourPrep.Modules.Auth.Application.Abstractions.Identity;

public sealed record LogoutUserEverywhereCommand(Guid UserId) : ICommand;

internal sealed class LogoutUserEverywhereCommandHandler(
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IUserSessionRepository sessionRepository,
    IIdentityProviderClient identityProviderClient) : ICommandHandler<LogoutUserEverywhereCommand>
{
    public async Task<Result> Handle(LogoutUserEverywhereCommand request, CancellationToken cancellationToken)
    {
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
