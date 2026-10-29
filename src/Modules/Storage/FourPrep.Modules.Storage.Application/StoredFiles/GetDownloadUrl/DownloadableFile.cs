namespace FourPrep.Modules.Storage.Application.StoredFiles.GetDownloadUrl;

using FourPrep.Modules.Storage.Domain.StoredFiles;

internal sealed record DownloadableFile
{
    public string ObjectKey { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public Guid UploadedByUserId { get; init; }
    public StoredFileStatus Status { get; init; }
    public ScanStatus ScanStatus { get; init; }
}
