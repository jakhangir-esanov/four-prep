namespace FourPrep.Modules.Storage.Application.StoredFiles.RemoveAbandonedUploads;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Storage.Domain.StoredFiles;
using FourPrep.Modules.Storage.Application.Abstractions.Data;
using FourPrep.Modules.Storage.Application.Abstractions.ObjectStorage;

public sealed record RemoveAbandonedUploadsCommand : ICommand<int>;

internal sealed class RemoveAbandonedUploadsCommandHandler(
    IUnitOfWork unitOfWork,
    IObjectStorage objectStorage,
    IStoredFileRepository repository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<RemoveAbandonedUploadsCommand, int>
{
    internal static readonly TimeSpan KeptUnconfirmed = TimeSpan.FromHours(1);

    internal const int BatchSize = 500;

    public async Task<Result<int>> Handle(RemoveAbandonedUploadsCommand request, CancellationToken cancellationToken)
    {
        var utcNow = dateTimeProvider.UtcNow;
        var abandoned = await repository.GetPendingCreatedBeforeAsync(utcNow - KeptUnconfirmed, BatchSize, cancellationToken);
        if (abandoned.Count == 0)
        {
            return 0;
        }

        foreach (var storedFile in abandoned)
        {
            await objectStorage.DeleteAsync(storedFile.UploadKey, cancellationToken);
            await objectStorage.DeleteAsync(storedFile.ObjectKey, cancellationToken);
            storedFile.Delete(utcNow);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return abandoned.Count;
    }
}
