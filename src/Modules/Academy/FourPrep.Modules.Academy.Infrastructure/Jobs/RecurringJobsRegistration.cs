namespace FourPrep.Modules.Academy.Infrastructure.Jobs;

using Hangfire;
using Microsoft.Extensions.Hosting;

internal sealed class RecurringJobsRegistration(IRecurringJobManager recurringJobManager) : IHostedService
{
    private const string EveryFiveMinutes = "*/5 * * * *";

    public Task StartAsync(CancellationToken cancellationToken)
    {
        recurringJobManager.AddOrUpdate<ExpireUnpaidEnrolmentsJob>(
            ExpireUnpaidEnrolmentsJob.JobId,
            job => job.RunAsync(CancellationToken.None),
            EveryFiveMinutes);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
