namespace FourPrep.Modules.Storage.Application.StoredFiles.DeleteFile;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Storage.Domain.StoredFiles;
using FourPrep.Modules.Storage.Application.Abstractions.Data;
using FourPrep.Modules.Storage.Application.Abstractions.ObjectStorage;

public sealed record DeleteFileCommand(Guid FileId) : ICommand;

internal sealed class DeleteFileCommandHandler(
    IUnitOfWork unitOfWork,
    IObjectStorage objectStorage,
    IStoredFileRepository repository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<DeleteFileCommand>
{
    public async Task<Result> Handle(DeleteFileCommand request, CancellationToken cancellationToken)
    {
        var storedFile = await repository.GetAsync(request.FileId, cancellationToken);
        if (storedFile is null)
        {
            return Result.Failure(StoredFileErrors.NotFound);
        }

        if (storedFile.Status == StoredFileStatus.Deleted)
        {
            return Result.Success();
        }

        await objectStorage.DeleteAsync(storedFile.UploadKey, cancellationToken);
        await objectStorage.DeleteAsync(storedFile.ObjectKey, cancellationToken);
        if (storedFile.PublicKey is not null)
        {
            await objectStorage.DeletePublicAsync(storedFile.PublicKey, cancellationToken);
        }

        storedFile.Delete(dateTimeProvider.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
