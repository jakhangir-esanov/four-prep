namespace FourPrep.Modules.Storage.Application.StoredFiles.CreateUploadUrl;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Storage.Domain.StoredFiles;
using FourPrep.Modules.Storage.Application.Abstractions.Data;
using FourPrep.Modules.Storage.Application.Abstractions.ObjectStorage;
using FourPrep.Modules.Storage.Application.Abstractions.Authentication;

public sealed record CreateUploadUrlCommand(string FileName, string ContentType, long SizeBytes) : ICommand<UploadUrlResponse>;

internal sealed class CreateUploadUrlCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IObjectStorage objectStorage,
    IStoredFileRepository repository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<CreateUploadUrlCommand, UploadUrlResponse>
{
    private const string ContentTypeHeader = "Content-Type";

    public async Task<Result<UploadUrlResponse>> Handle(CreateUploadUrlCommand request, CancellationToken cancellationToken)
    {
        if (await repository.CountPendingAsync(currentUser.UserId, cancellationToken) >= StoredFile.MaxPendingPerUser)
        {
            return Result.Failure<UploadUrlResponse>(StoredFileErrors.TooManyPending);
        }

        var utcNow = dateTimeProvider.UtcNow;

        var storedFile = StoredFile.Create(
            FileUploadRules.SafeFileName(request.FileName),
            FileUploadRules.NormalizedType(request.ContentType),
            request.SizeBytes,
            currentUser.UserId,
            utcNow);

        repository.Insert(storedFile);

        var expiresOnUtc = utcNow + StoredFile.UploadWindow;
        var uploadUrl = await objectStorage.CreateUploadUrlAsync(storedFile.UploadKey, storedFile.ContentType, expiresOnUtc);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UploadUrlResponse(
            storedFile.Id,
            uploadUrl,
            HttpMethod.Put.Method,
            new Dictionary<string, string> { [ContentTypeHeader] = storedFile.ContentType },
            expiresOnUtc);
    }
}
