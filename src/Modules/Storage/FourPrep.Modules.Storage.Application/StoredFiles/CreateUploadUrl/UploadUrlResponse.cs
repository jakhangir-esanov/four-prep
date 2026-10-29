namespace FourPrep.Modules.Storage.Application.StoredFiles.CreateUploadUrl;

public sealed record UploadUrlResponse(
    Guid FileId,
    string UploadUrl,
    string Method,
    IReadOnlyDictionary<string, string> Headers,
    DateTime ExpiresOnUtc);
