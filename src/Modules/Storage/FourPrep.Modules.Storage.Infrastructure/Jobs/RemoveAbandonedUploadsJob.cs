namespace FourPrep.Modules.Storage.Infrastructure.Jobs;

using MediatR;
using FourPrep.Modules.Storage.Application.StoredFiles.RemoveAbandonedUploads;

public sealed class RemoveAbandonedUploadsJob(ISender sender)
{
    public const string JobId = "storage-remove-abandoned-uploads";

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        await sender.Send(new RemoveAbandonedUploadsCommand(), cancellationToken);
    }
}
