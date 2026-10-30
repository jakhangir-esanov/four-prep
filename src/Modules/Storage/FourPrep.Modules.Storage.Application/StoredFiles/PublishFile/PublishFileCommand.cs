namespace FourPrep.Modules.Storage.Application.StoredFiles.PublishFile;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Storage.Domain.StoredFiles;
using FourPrep.Modules.Storage.Application.Abstractions.Data;
using FourPrep.Modules.Storage.Application.Abstractions.ObjectStorage;

public sealed record PublishFileCommand(Guid FileId) : ICommand<string>;

internal sealed class PublishFileCommandHandler(
    IUnitOfWork unitOfWork,
    IObjectStorage objectStorage,
    IStoredFileRepository repository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<PublishFileCommand, string>
{
    public async Task<Result<string>> Handle(PublishFileCommand request, CancellationToken cancellationToken)
    {
        var storedFile = await repository.GetAsync(request.FileId, cancellationToken);
        if (storedFile is null)
        {
            return Result.Failure<string>(StoredFileErrors.NotFound);
        }

        if (storedFile.PublicKey is not null)
        {
            return objectStorage.GetPublicUrl(storedFile.PublicKey);
        }

        var published = storedFile.Publish(dateTimeProvider.UtcNow);
        if (published.IsFailure)
        {
            return Result.Failure<string>(published.Error);
        }

        var publicKey = storedFile.PublicKey!;

        await objectStorage.CopyToPublicAsync(storedFile.ObjectKey, publicKey, storedFile.ContentType, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return objectStorage.GetPublicUrl(publicKey);
    }
}
