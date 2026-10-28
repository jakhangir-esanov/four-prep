namespace FourPrep.Modules.Auth.Infrastructure.Jobs;

using MediatR;
using FourPrep.Modules.Auth.Application.UserSessions.PurgeExpiredSessions;

public sealed class PurgeExpiredSessionsJob(ISender sender)
{
    public const string JobId = "auth-purge-expired-sessions";

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        await sender.Send(new PurgeExpiredSessionsCommand(), cancellationToken);
    }
}
