namespace FourPrep.Modules.Storage.Infrastructure.StoredFiles;

using Microsoft.EntityFrameworkCore;
using FourPrep.Modules.Storage.Domain.StoredFiles;
using FourPrep.Modules.Storage.Infrastructure.Database;

internal sealed class StoredFileRepository(StorageDbContext dbContext) : IStoredFileRepository
{
    public async Task<StoredFile?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.StoredFiles.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<StoredFile>> GetPendingCreatedBeforeAsync(
        DateTime cutoffUtc,
        int limit,
        CancellationToken cancellationToken = default)
    {
        DateTimeOffset cutoff = cutoffUtc;

        return await dbContext.StoredFiles
            .Where(x => x.Status == StoredFileStatus.Pending && x.CreatedOnUtc < cutoff)
            .OrderBy(x => x.CreatedOnUtc)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StoredFile>> GetAwaitingScanAsync(int limit, CancellationToken cancellationToken = default)
    {
        return await dbContext.StoredFiles
            .Where(x => x.Status == StoredFileStatus.Uploaded && x.ScanStatus == ScanStatus.Pending)
            .OrderBy(x => x.ScanAttempts)
            .ThenBy(x => x.UploadedOnUtc)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountPendingAsync(Guid uploadedByUserId, CancellationToken cancellationToken = default)
    {
        return await dbContext.StoredFiles.CountAsync(
            x => x.UploadedByUserId == uploadedByUserId && x.Status == StoredFileStatus.Pending,
            cancellationToken);
    }

    public void Insert(StoredFile storedFile)
    {
        dbContext.StoredFiles.Add(storedFile);
    }
}
