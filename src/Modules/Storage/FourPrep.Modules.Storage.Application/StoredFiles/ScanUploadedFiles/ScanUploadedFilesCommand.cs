namespace FourPrep.Modules.Storage.Application.StoredFiles.ScanUploadedFiles;

using System.Security.Cryptography;
using FourPrep.Common.Domain;
using Microsoft.Extensions.Logging;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Storage.Domain.StoredFiles;
using FourPrep.Modules.Storage.Application.Abstractions.Data;
using FourPrep.Modules.Storage.Application.Abstractions.ObjectStorage;
using FourPrep.Modules.Storage.Application.Abstractions.VirusScanning;

public sealed record ScanUploadedFilesCommand : ICommand<int>;

internal sealed class ScanUploadedFilesCommandHandler(
    IUnitOfWork unitOfWork,
    IVirusScanner virusScanner,
    IObjectStorage objectStorage,
    IStoredFileRepository repository,
    IDateTimeProvider dateTimeProvider,
    ILogger<ScanUploadedFilesCommandHandler> logger) : ICommandHandler<ScanUploadedFilesCommand, int>
{
    internal const int BatchSize = 10;

    public async Task<Result<int>> Handle(ScanUploadedFilesCommand request, CancellationToken cancellationToken)
    {
        var awaiting = await repository.GetAwaitingScanAsync(BatchSize, cancellationToken);
        if (awaiting.Count == 0)
        {
            return 0;
        }

        foreach (var storedFile in awaiting)
        {
            await ScanOneAsync(storedFile, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return awaiting.Count;
    }

    private async Task ScanOneAsync(StoredFile storedFile, CancellationToken cancellationToken)
    {
        try
        {
            await ScanAsync(storedFile, cancellationToken);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            storedFile.RecordScanFailure(dateTimeProvider.UtcNow);

            logger.LogError(
                exception,
                "Could not scan file {FileId} (attempt {Attempt} of {MaxAttempts})",
                storedFile.Id,
                storedFile.ScanAttempts,
                StoredFile.MaxScanAttempts);
        }
    }

    private async Task ScanAsync(StoredFile storedFile, CancellationToken cancellationToken)
    {
        var content = await objectStorage.ReadAsync(storedFile.ObjectKey, cancellationToken);

        using var stream = new MemoryStream(content, writable: false);
        var result = await virusScanner.ScanAsync(stream, cancellationToken);

        if (result.IsClean)
        {
            storedFile.MarkClean(Convert.ToHexStringLower(SHA256.HashData(content)), dateTimeProvider.UtcNow);
            return;
        }

        storedFile.MarkInfected(dateTimeProvider.UtcNow);
        await objectStorage.DeleteAsync(storedFile.ObjectKey, cancellationToken);

        logger.LogWarning("Deleted infected file {FileId}: {Threat}", storedFile.Id, result.Threat);
    }
}
