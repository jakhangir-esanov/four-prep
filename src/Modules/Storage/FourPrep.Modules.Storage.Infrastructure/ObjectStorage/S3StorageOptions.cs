namespace FourPrep.Modules.Storage.Infrastructure.ObjectStorage;

public sealed class S3StorageOptions
{
    public const string SectionName = "Storage:S3";

    public const string IncompleteMessage =
        "Storage:S3 needs ServiceUrl, Region, AccessKey, SecretKey and DocumentsBucket.";

    public string ServiceUrl { get; init; } = string.Empty;
    public string Region { get; init; } = string.Empty;
    public string AccessKey { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public string DocumentsBucket { get; init; } = string.Empty;
    public bool ForcePathStyle { get; init; }

    public static bool IsComplete(S3StorageOptions options) =>
        new[] { options.ServiceUrl, options.Region, options.AccessKey, options.SecretKey, options.DocumentsBucket }
            .All(value => !string.IsNullOrWhiteSpace(value));
}
