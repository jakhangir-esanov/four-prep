namespace FourPrep.Modules.Auth.Application.UserSessions.PurgeExpiredSessions;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Auth.Domain.UserSessions;

public sealed record PurgeExpiredSessionsCommand : ICommand<int>;

internal sealed class PurgeExpiredSessionsCommandHandler(
    IDateTimeProvider dateTimeProvider,
    IUserSessionRepository sessionRepository) : ICommandHandler<PurgeExpiredSessionsCommand, int>
{
    private static readonly TimeSpan KeycloakIdleTimeout = TimeSpan.FromDays(30);

    public async Task<Result<int>> Handle(PurgeExpiredSessionsCommand request, CancellationToken cancellationToken)
    {
        return await sessionRepository.DeleteExpiredBeforeAsync(
            dateTimeProvider.UtcNow - KeycloakIdleTimeout,
            cancellationToken);
    }
}
