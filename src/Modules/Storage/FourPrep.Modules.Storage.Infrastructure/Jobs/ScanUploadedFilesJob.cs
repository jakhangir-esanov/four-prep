namespace FourPrep.Modules.Storage.Infrastructure.Jobs;

using MediatR;
using Hangfire;
using FourPrep.Modules.Storage.Application.StoredFiles.ScanUploadedFiles;

public sealed class ScanUploadedFilesJob(ISender sender)
{
    public const string JobId = "storage-scan-uploaded-files";

    [DisableConcurrentExecution(timeoutInSeconds: 900)]
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        await sender.Send(new ScanUploadedFilesCommand(), cancellationToken);
    }
}
