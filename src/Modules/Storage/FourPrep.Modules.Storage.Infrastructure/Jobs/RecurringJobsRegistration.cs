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

        recurringJobManager.AddOrUpdate<ScanUploadedFilesJob>(
            ScanUploadedFilesJob.JobId,
            job => job.RunAsync(CancellationToken.None),
            Cron.Minutely());

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
