namespace FourPrep.Modules.Storage.Infrastructure.ObjectStorage;

public sealed class S3StorageOptions
{
    public const string SectionName = "Storage:S3";

    public const string IncompleteMessage =
        "Storage:S3 needs ServiceUrl, Region, AccessKey, SecretKey, DocumentsBucket, PublicBucket and PublicBaseUrl.";

    public const string DocumentsNotPrivateMessage =
        "Storage:S3 PublicBucket and PublicBaseUrl must point at a bucket other than DocumentsBucket, " +
        "and PublicBaseUrl must be an absolute http(s) address.";

    public string ServiceUrl { get; init; } = string.Empty;
    public string Region { get; init; } = string.Empty;
    public string AccessKey { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public string DocumentsBucket { get; init; } = string.Empty;
    public string PublicBucket { get; init; } = string.Empty;
    public string PublicBaseUrl { get; init; } = string.Empty;
    public bool ForcePathStyle { get; init; }

    public static bool IsComplete(S3StorageOptions options) =>
        new[]
            {
                options.ServiceUrl,
                options.Region,
                options.AccessKey,
                options.SecretKey,
                options.DocumentsBucket,
                options.PublicBucket,
                options.PublicBaseUrl
            }
            .All(value => !string.IsNullOrWhiteSpace(value));

    public static bool KeepsDocumentsPrivate(S3StorageOptions options) =>
        !string.Equals(options.PublicBucket.Trim(), options.DocumentsBucket.Trim(), StringComparison.OrdinalIgnoreCase) &&
        Uri.TryCreate(options.PublicBaseUrl, UriKind.Absolute, out var baseUrl) &&
        (baseUrl.Scheme == Uri.UriSchemeHttps || baseUrl.Scheme == Uri.UriSchemeHttp) &&
        !NamesBucket(baseUrl, options.DocumentsBucket.Trim());

    private static bool NamesBucket(Uri url, string bucket) =>
        url.Host.Split('.')[0].Equals(bucket, StringComparison.OrdinalIgnoreCase) ||
        url.Segments.Any(segment => segment.Trim('/').Equals(bucket, StringComparison.OrdinalIgnoreCase));
}
