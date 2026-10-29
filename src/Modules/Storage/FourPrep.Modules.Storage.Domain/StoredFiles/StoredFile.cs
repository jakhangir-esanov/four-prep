namespace FourPrep.Modules.Storage.Domain.StoredFiles;

using FourPrep.Common.Domain;

public sealed class StoredFile
{
    public const int MaxPendingPerUser = 10;

    public static readonly TimeSpan UploadWindow = TimeSpan.FromMinutes(10);

    public static readonly TimeSpan DownloadWindow = TimeSpan.FromMinutes(5);

    private const string UploadKeyPrefix = "uploads/";
    private const string ObjectKeyPrefix = "docs/";

    public Guid Id { get; set; }
    public string UploadKey { get; set; } = string.Empty;
    public string ObjectKey { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public Guid UploadedByUserId { get; set; }
    public StoredFileStatus Status { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? UploadedOnUtc { get; set; }
    public DateTimeOffset? DeletedOnUtc { get; set; }

    public static StoredFile Create(
        string fileName,
        string contentType,
        long sizeBytes,
        Guid uploadedByUserId,
        DateTime utcNow)
    {
        return new StoredFile
        {
            Id = Guid.NewGuid(),
            UploadKey = $"{UploadKeyPrefix}{Guid.NewGuid():N}",
            ObjectKey = $"{ObjectKeyPrefix}{Guid.NewGuid():N}",
            FileName = fileName,
            ContentType = contentType,
            SizeBytes = sizeBytes,
            UploadedByUserId = uploadedByUserId,
            Status = StoredFileStatus.Pending,
            CreatedOnUtc = utcNow
        };
    }

    public Result CompleteUpload(long storedSizeBytes, DateTime utcNow)
    {
        switch (Status)
        {
            case StoredFileStatus.Deleted:
                return Result.Failure(StoredFileErrors.NotFound);
            case StoredFileStatus.Uploaded:
                return Result.Failure(StoredFileErrors.AlreadyUploaded);
        }

        if (storedSizeBytes != SizeBytes)
        {
            return Result.Failure(StoredFileErrors.SizeMismatch);
        }

        Status = StoredFileStatus.Uploaded;
        UploadedOnUtc = utcNow;

        return Result.Success();
    }

    public void Delete(DateTime utcNow)
    {
        if (Status == StoredFileStatus.Deleted)
        {
            return;
        }

        Status = StoredFileStatus.Deleted;
        DeletedOnUtc = utcNow;
    }
}
