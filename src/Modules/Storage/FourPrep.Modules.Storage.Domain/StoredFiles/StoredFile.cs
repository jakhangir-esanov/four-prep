namespace FourPrep.Modules.Storage.Domain.StoredFiles;

using FourPrep.Common.Domain;

public sealed class StoredFile
{
    public const int MaxPendingPerUser = 10;

    public const int MaxScanAttempts = 3;

    public static readonly TimeSpan UploadWindow = TimeSpan.FromMinutes(10);

    public static readonly TimeSpan DownloadWindow = TimeSpan.FromMinutes(5);

    private const string UploadKeyPrefix = "uploads/";
    private const string ObjectKeyPrefix = "docs/";
    private const string PublicKeyPrefix = "public/";

    public Guid Id { get; set; }
    public string UploadKey { get; set; } = string.Empty;
    public string ObjectKey { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public Guid UploadedByUserId { get; set; }
    public StoredFileStatus Status { get; set; }
    public ScanStatus ScanStatus { get; set; }
    public string? Sha256 { get; set; }
    public DateTimeOffset? ScannedOnUtc { get; set; }
    public int ScanAttempts { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? UploadedOnUtc { get; set; }
    public DateTimeOffset? DeletedOnUtc { get; set; }
    public string? PublicKey { get; set; }
    public DateTimeOffset? PublishedOnUtc { get; set; }

    public bool IsAwaitingScan => Status == StoredFileStatus.Uploaded && ScanStatus == ScanStatus.Pending;

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
        ScanStatus = ScanStatus.Pending;
        UploadedOnUtc = utcNow;

        return Result.Success();
    }

    public Result MarkClean(string sha256, DateTime utcNow)
    {
        if (!IsAwaitingScan)
        {
            return Result.Failure(StoredFileErrors.NotAwaitingScan);
        }

        ScanStatus = ScanStatus.Clean;
        Sha256 = sha256;
        ScannedOnUtc = utcNow;

        return Result.Success();
    }

    public Result MarkInfected(DateTime utcNow)
    {
        if (!IsAwaitingScan)
        {
            return Result.Failure(StoredFileErrors.NotAwaitingScan);
        }

        ScanStatus = ScanStatus.Infected;
        ScannedOnUtc = utcNow;
        Delete(utcNow);

        return Result.Success();
    }

    public void RecordScanFailure(DateTime utcNow)
    {
        if (!IsAwaitingScan)
        {
            return;
        }

        ScanAttempts++;

        if (ScanAttempts >= MaxScanAttempts)
        {
            ScanStatus = ScanStatus.Failed;
            ScannedOnUtc = utcNow;
        }
    }

    public Result Publish(DateTime utcNow)
    {
        if (Status != StoredFileStatus.Uploaded)
        {
            return Result.Failure(StoredFileErrors.NotFound);
        }

        switch (ScanStatus)
        {
            case ScanStatus.Failed:
                return Result.Failure(StoredFileErrors.ScanFailed);
            case not ScanStatus.Clean:
                return Result.Failure(StoredFileErrors.NotScanned);
        }

        if (PublicKey is not null)
        {
            return Result.Success();
        }

        PublicKey = $"{PublicKeyPrefix}{Guid.NewGuid():N}";
        PublishedOnUtc = utcNow;

        return Result.Success();
    }

    public void Unpublish()
    {
        PublicKey = null;
        PublishedOnUtc = null;
    }

    public void Delete(DateTime utcNow)
    {
        if (Status == StoredFileStatus.Deleted)
        {
            return;
        }

        Status = StoredFileStatus.Deleted;
        DeletedOnUtc = utcNow;
        Unpublish();
    }
}
