namespace FourPrep.Modules.Content.Storage;

using MediatR;
using FourPrep.Common.Domain;
using FourPrep.Modules.Content.Application.Abstractions.Storage;
using FourPrep.Modules.Storage.Application.StoredFiles.PublishFile;
using FourPrep.Modules.Storage.Application.StoredFiles.GetStoredFile;
using FourPrep.Modules.Storage.Application.StoredFiles.UnpublishFile;

public sealed class StorageService(ISender sender) : IStorageService
{
    public async Task<Result<StorageFile>> GetFileAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var file = await sender.Send(new GetStoredFileQuery(fileId), cancellationToken);

        return file.IsSuccess
            ? new StorageFile(file.Data.Id, file.Data.ContentType, file.Data.UploadedByUserId)
            : Result.Failure<StorageFile>(file.Error);
    }

    public async Task<Result<string>> PublishFileAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        return await sender.Send(new PublishFileCommand(fileId), cancellationToken);
    }

    public async Task<Result> UnpublishFileAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        return await sender.Send(new UnpublishFileCommand(fileId), cancellationToken);
    }
}
