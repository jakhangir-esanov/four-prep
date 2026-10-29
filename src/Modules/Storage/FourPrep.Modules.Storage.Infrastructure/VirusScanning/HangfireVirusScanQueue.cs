namespace FourPrep.Modules.Storage.Infrastructure.VirusScanning;

using Hangfire;
using FourPrep.Modules.Storage.Infrastructure.Jobs;
using FourPrep.Modules.Storage.Application.Abstractions.VirusScanning;

internal sealed class HangfireVirusScanQueue(IBackgroundJobClient backgroundJobClient) : IVirusScanQueue
{
    public void RequestScan()
    {
        backgroundJobClient.Enqueue<ScanUploadedFilesJob>(job => job.RunAsync(CancellationToken.None));
    }
}
