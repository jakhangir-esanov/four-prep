namespace FourPrep.Modules.Storage.Application.StoredFiles.CompleteUpload;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Storage.Domain.StoredFiles;
using FourPrep.Modules.Storage.Application.Abstractions.Data;
using FourPrep.Modules.Storage.Application.Abstractions.ObjectStorage;
using FourPrep.Modules.Storage.Application.Abstractions.VirusScanning;
using FourPrep.Modules.Storage.Application.Abstractions.Authentication;

public sealed record CompleteUploadCommand(Guid FileId) : ICommand;

internal sealed class CompleteUploadCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IObjectStorage objectStorage,
    IVirusScanQueue virusScanQueue,
    IStoredFileRepository repository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<CompleteUploadCommand>
{
    public async Task<Result> Handle(CompleteUploadCommand request, CancellationToken cancellationToken)
    {
        var storedFile = await repository.GetAsync(request.FileId, cancellationToken);
        if (storedFile is null ||
            storedFile.UploadedByUserId != currentUser.UserId ||
            storedFile.Status == StoredFileStatus.Deleted)
        {
            return Result.Failure(StoredFileErrors.NotFound);
        }

        if (storedFile.Status == StoredFileStatus.Uploaded)
        {
            return Result.Failure(StoredFileErrors.AlreadyUploaded);
        }

        var storedSize = await objectStorage.GetSizeAsync(storedFile.UploadKey, cancellationToken);
        if (storedSize is null)
        {
            return Result.Failure(StoredFileErrors.NotUploaded);
        }

        var checkedUpload = await CheckUploadAsync(storedFile, storedSize.Value, cancellationToken);
        if (checkedUpload.IsFailure)
        {
            await objectStorage.DeleteAsync(storedFile.UploadKey, cancellationToken);
            return checkedUpload;
        }

        await objectStorage.MoveAsync(storedFile.UploadKey, storedFile.ObjectKey, cancellationToken);

        var completed = storedFile.CompleteUpload(storedSize.Value, dateTimeProvider.UtcNow);
        if (completed.IsFailure)
        {
            return completed;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        virusScanQueue.RequestScan();

        return Result.Success();
    }

    private async Task<Result> CheckUploadAsync(StoredFile storedFile, long storedSize, CancellationToken cancellationToken)
    {
        if (storedSize != storedFile.SizeBytes)
        {
            return Result.Failure(StoredFileErrors.SizeMismatch);
        }

        var header = await objectStorage.ReadStartAsync(storedFile.UploadKey, FileUploadRules.HeaderLength, cancellationToken);

        return FileUploadRules.MatchesSignature(storedFile.ContentType, header)
            ? Result.Success()
            : Result.Failure(StoredFileErrors.ContentMismatch);
    }
}
