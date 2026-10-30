namespace FourPrep.Modules.Storage.Domain.StoredFiles;

public interface IStoredFileRepository
{
    Task<StoredFile?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StoredFile>> GetPendingCreatedBeforeAsync(
        DateTime cutoffUtc,
        int limit,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StoredFile>> GetAwaitingScanAsync(int limit, CancellationToken cancellationToken = default);

    Task<IReadOnlySet<string>> GetPublicKeysInUseAsync(
        IReadOnlyCollection<string> publicKeys,
        CancellationToken cancellationToken = default);

    Task<int> CountPendingAsync(Guid uploadedByUserId, CancellationToken cancellationToken = default);

    void Insert(StoredFile storedFile);
}
