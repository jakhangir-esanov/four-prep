namespace FourPrep.Modules.Storage.UnitTests;

using System.Data.Common;
using FourPrep.Common.Application.Clock;
using FourPrep.Modules.Storage.Domain.StoredFiles;
using FourPrep.Modules.Storage.Application.Abstractions.Data;
using FourPrep.Modules.Storage.Application.Abstractions.ObjectStorage;
using FourPrep.Modules.Storage.Application.Abstractions.Authentication;

internal sealed class FakeClock : IDateTimeProvider
{
    public DateTime UtcNow { get; } = new(2026, 10, 29, 9, 0, 0, DateTimeKind.Utc);
}

internal sealed class FakeCurrentUser(Guid userId) : ICurrentUser
{
    public Guid UserId { get; } = userId;
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveCount++;
        return Task.FromResult(0);
    }

    public Task<DbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();
}

internal sealed class FakeStoredFileRepository : IStoredFileRepository
{
    public List<StoredFile> Files { get; } = [];

    public StoredFile Add(Guid uploadedByUserId, string fileName = "passport.pdf", string contentType = "application/pdf", long sizeBytes = 64)
    {
        var storedFile = StoredFile.Create(fileName, contentType, sizeBytes, uploadedByUserId, new DateTime(2026, 10, 29, 8, 0, 0, DateTimeKind.Utc));
        Files.Add(storedFile);
        return storedFile;
    }

    public Task<StoredFile?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Files.SingleOrDefault(x => x.Id == id));

    public Task<IReadOnlyList<StoredFile>> GetPendingCreatedBeforeAsync(
        DateTime cutoffUtc,
        int limit,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<StoredFile>>(
            [.. Files.Where(x => x.Status == StoredFileStatus.Pending && x.CreatedOnUtc < cutoffUtc).Take(limit)]);

    public Task<int> CountPendingAsync(Guid uploadedByUserId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Files.Count(x => x.UploadedByUserId == uploadedByUserId && x.Status == StoredFileStatus.Pending));

    public void Insert(StoredFile storedFile) => Files.Add(storedFile);
}

internal sealed class FakeObjectStorage : IObjectStorage
{
    public Dictionary<string, byte[]> Objects { get; } = [];

    public List<string> Deleted { get; } = [];

    public int HeaderReads { get; private set; }

    public Task<string> CreateUploadUrlAsync(string objectKey, string contentType, DateTime expiresOnUtc) =>
        Task.FromResult($"https://storage.test/{objectKey}?put&type={contentType}&expires={expiresOnUtc:O}");

    public Task<string> CreateDownloadUrlAsync(string objectKey, string fileName, string contentType, DateTime expiresOnUtc) =>
        Task.FromResult($"https://storage.test/{objectKey}?get&name={fileName}&expires={expiresOnUtc:O}");

    public Task<long?> GetSizeAsync(string objectKey, CancellationToken cancellationToken = default) =>
        Task.FromResult(Objects.TryGetValue(objectKey, out var bytes) ? (long?)bytes.Length : null);

    public Task<byte[]> ReadStartAsync(string objectKey, int length, CancellationToken cancellationToken = default)
    {
        HeaderReads++;
        return Task.FromResult(Objects[objectKey].Take(length).ToArray());
    }

    public Task MoveAsync(string sourceKey, string targetKey, CancellationToken cancellationToken = default)
    {
        Objects[targetKey] = Objects[sourceKey];
        Objects.Remove(sourceKey);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        Objects.Remove(objectKey);
        Deleted.Add(objectKey);
        return Task.CompletedTask;
    }
}

internal static class FileSamples
{
    public static readonly byte[] Pdf = [.. "%PDF-1.7\n"u8, .. new byte[55]];

    public static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, .. new byte[56]];

    public static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF, 0xE0, .. new byte[60]];

    public static readonly byte[] Heic = [0x00, 0x00, 0x00, 0x18, .. "ftypheic"u8, .. new byte[52]];
}
