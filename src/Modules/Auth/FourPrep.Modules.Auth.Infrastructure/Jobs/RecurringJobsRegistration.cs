namespace FourPrep.Modules.Auth.Infrastructure.Jobs;

using Hangfire;
using Microsoft.Extensions.Hosting;

internal sealed class RecurringJobsRegistration(IRecurringJobManager recurringJobManager) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        recurringJobManager.AddOrUpdate<PurgeExpiredSessionsJob>(
            PurgeExpiredSessionsJob.JobId,
            job => job.RunAsync(CancellationToken.None),
            Cron.Daily());

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
