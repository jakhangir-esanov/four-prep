namespace FourPrep.Modules.Storage.Application.StoredFiles.UnpublishFile;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Storage.Domain.StoredFiles;
using FourPrep.Modules.Storage.Application.Abstractions.Data;
using FourPrep.Modules.Storage.Application.Abstractions.ObjectStorage;

public sealed record UnpublishFileCommand(Guid FileId) : ICommand;

internal sealed class UnpublishFileCommandHandler(
    IUnitOfWork unitOfWork,
    IObjectStorage objectStorage,
    IStoredFileRepository repository) : ICommandHandler<UnpublishFileCommand>
{
    public async Task<Result> Handle(UnpublishFileCommand request, CancellationToken cancellationToken)
    {
        var storedFile = await repository.GetAsync(request.FileId, cancellationToken);
        if (storedFile is null)
        {
            return Result.Failure(StoredFileErrors.NotFound);
        }

        if (storedFile.PublicKey is null)
        {
            return Result.Success();
        }

        await objectStorage.DeletePublicAsync(storedFile.PublicKey, cancellationToken);
        storedFile.Unpublish();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
