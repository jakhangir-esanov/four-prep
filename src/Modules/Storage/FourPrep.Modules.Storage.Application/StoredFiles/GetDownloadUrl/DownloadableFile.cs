namespace FourPrep.Modules.Storage.Application.StoredFiles.GetDownloadUrl;

internal sealed record DownloadableFile
{
    public string ObjectKey { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
}
