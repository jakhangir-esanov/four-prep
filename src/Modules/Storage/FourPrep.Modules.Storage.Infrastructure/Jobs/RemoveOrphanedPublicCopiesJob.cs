namespace FourPrep.Modules.Storage.Infrastructure.Jobs;

using MediatR;
using Hangfire;
using FourPrep.Modules.Storage.Application.StoredFiles.RemoveOrphanedPublicCopies;

public sealed class RemoveOrphanedPublicCopiesJob(ISender sender)
{
    public const string JobId = "storage-remove-orphaned-public-copies";

    [AutomaticRetry(Attempts = 0)]
    [DisableConcurrentExecution(timeoutInSeconds: 3600)]
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RemoveOrphanedPublicCopiesCommand(), cancellationToken);
        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error.Message);
        }
    }
}
