namespace FourPrep.Modules.Storage.Presentation.StoredFiles.Requests;

using FourPrep.Modules.Storage.Domain.StoredFiles;

internal sealed record CreateUploadUrlRequest
{
    public string? FileName { get; init; }
    public string? ContentType { get; init; }
    public long SizeBytes { get; init; }
    public StoredFilePurpose? Purpose { get; init; }
}
