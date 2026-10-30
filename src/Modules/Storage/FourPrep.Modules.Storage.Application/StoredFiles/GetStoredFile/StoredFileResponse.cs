namespace FourPrep.Modules.Storage.Application.StoredFiles.GetStoredFile;

using FourPrep.Modules.Storage.Domain.StoredFiles;

public sealed record StoredFileResponse
{
    public Guid Id { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long SizeBytes { get; init; }
    public StoredFilePurpose Purpose { get; init; }
    public Guid UploadedByUserId { get; init; }
    public StoredFileStatus Status { get; init; }
    public ScanStatus ScanStatus { get; init; }
    public string? Sha256 { get; init; }
    public DateTime? UploadedOnUtc { get; init; }
    public DateTime? DeletedOnUtc { get; init; }

    public bool IsReady => Status == StoredFileStatus.Uploaded && ScanStatus == ScanStatus.Clean;
}
