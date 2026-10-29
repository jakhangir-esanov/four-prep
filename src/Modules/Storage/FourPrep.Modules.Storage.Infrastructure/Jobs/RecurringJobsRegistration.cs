namespace FourPrep.Modules.Storage.Infrastructure.Jobs;

using Hangfire;
using Microsoft.Extensions.Hosting;

internal sealed class RecurringJobsRegistration(IRecurringJobManager recurringJobManager) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        recurringJobManager.AddOrUpdate<RemoveAbandonedUploadsJob>(
            RemoveAbandonedUploadsJob.JobId,
            job => job.RunAsync(CancellationToken.None),
            Cron.Hourly());

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
