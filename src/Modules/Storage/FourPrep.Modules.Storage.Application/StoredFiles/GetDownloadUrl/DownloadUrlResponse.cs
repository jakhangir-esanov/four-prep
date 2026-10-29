namespace FourPrep.Modules.Storage.Application.StoredFiles.GetDownloadUrl;

public sealed record DownloadUrlResponse(string DownloadUrl, DateTime ExpiresOnUtc);
