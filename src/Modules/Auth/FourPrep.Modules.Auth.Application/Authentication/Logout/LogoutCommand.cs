namespace FourPrep.Modules.Auth.Application.Authentication.Logout;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Auth.Domain.UserSessions;
using FourPrep.Modules.Auth.Application.Abstractions.Data;
using FourPrep.Modules.Auth.Application.Abstractions.Identity;

public sealed record LogoutCommand(string RefreshToken) : ICommand;

internal sealed class LogoutCommandHandler(
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IUserSessionRepository sessionRepository,
    IIdentityProviderClient identityProviderClient) : ICommandHandler<LogoutCommand>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var loggedOut = await identityProviderClient.LogoutAsync(request.RefreshToken, cancellationToken);
        if (loggedOut.IsFailure)
        {
            return loggedOut;
        }

        string? sessionId = identityProviderClient.ReadSessionId(request.RefreshToken);
        if (string.IsNullOrEmpty(sessionId))
        {
            return Result.Success();
        }

        var session = await sessionRepository.GetByIdentitySessionIdAsync(sessionId, cancellationToken);
        if (session is { IsRevoked: false })
        {
            session.Revoke(dateTimeProvider.UtcNow);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
