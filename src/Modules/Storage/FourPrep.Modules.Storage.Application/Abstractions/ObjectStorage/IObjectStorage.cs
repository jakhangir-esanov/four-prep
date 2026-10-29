namespace FourPrep.Modules.Storage.Application.Abstractions.ObjectStorage;

public interface IObjectStorage
{
    Task<string> CreateUploadUrlAsync(string objectKey, string contentType, DateTime expiresOnUtc);

    Task<string> CreateDownloadUrlAsync(string objectKey, string fileName, string contentType, DateTime expiresOnUtc);

    Task<long?> GetSizeAsync(string objectKey, CancellationToken cancellationToken = default);

    Task<byte[]> ReadStartAsync(string objectKey, int length, CancellationToken cancellationToken = default);

    Task MoveAsync(string sourceKey, string targetKey, CancellationToken cancellationToken = default);

    Task DeleteAsync(string objectKey, CancellationToken cancellationToken = default);
}
