namespace FourPrep.Modules.Content.Application.Abstractions.Storage;

using FourPrep.Common.Domain;

public interface IStorageService
{
    Task<Result<StorageFile>> GetFileAsync(Guid fileId, CancellationToken cancellationToken = default);

    Task<Result<string>> PublishFileAsync(Guid fileId, CancellationToken cancellationToken = default);

    Task<Result> UnpublishFileAsync(Guid fileId, CancellationToken cancellationToken = default);
}
