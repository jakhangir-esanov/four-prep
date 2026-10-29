namespace FourPrep.Modules.Storage.Presentation.StoredFiles.Requests;

internal sealed record CreateUploadUrlRequest
{
    public string? FileName { get; init; }
    public string? ContentType { get; init; }
    public long SizeBytes { get; init; }
}
