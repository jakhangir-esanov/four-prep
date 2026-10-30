namespace FourPrep.Modules.Storage.Infrastructure.ObjectStorage;

using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using FourPrep.Modules.Storage.Application.Abstractions.ObjectStorage;

internal sealed class S3ObjectStorage(IAmazonS3 s3, IOptions<S3StorageOptions> options) : IObjectStorage
{
    private const string PublicCacheControl = "public, max-age=86400";

    private readonly S3StorageOptions _options = options.Value;

    private Protocol UrlProtocol =>
        Uri.TryCreate(_options.ServiceUrl, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttp
            ? Protocol.HTTP
            : Protocol.HTTPS;

    public Task<string> CreateUploadUrlAsync(string objectKey, string contentType, DateTime expiresOnUtc) =>
        s3.GetPreSignedURLAsync(new GetPreSignedUrlRequest
        {
            BucketName = _options.DocumentsBucket,
            Key = objectKey,
            Verb = HttpVerb.PUT,
            ContentType = contentType,
            Expires = expiresOnUtc,
            Protocol = UrlProtocol
        });

    public Task<string> CreateDownloadUrlAsync(string objectKey, string fileName, string contentType, DateTime expiresOnUtc)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _options.DocumentsBucket,
            Key = objectKey,
            Verb = HttpVerb.GET,
            Expires = expiresOnUtc,
            Protocol = UrlProtocol
        };

        request.ResponseHeaderOverrides.ContentType = contentType;
        request.ResponseHeaderOverrides.ContentDisposition = $"attachment; filename*=UTF-8''{Uri.EscapeDataString(fileName)}";

        return s3.GetPreSignedURLAsync(request);
    }

    public async Task<long?> GetSizeAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var metadata = await s3.GetObjectMetadataAsync(_options.DocumentsBucket, objectKey, cancellationToken);
            return metadata.ContentLength;
        }
        catch (AmazonS3Exception exception) when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<byte[]> ReadStartAsync(string objectKey, int length, CancellationToken cancellationToken = default)
    {
        using var response = await s3.GetObjectAsync(
            new GetObjectRequest
            {
                BucketName = _options.DocumentsBucket,
                Key = objectKey,
                ByteRange = new ByteRange(0, length - 1)
            },
            cancellationToken);

        using var buffer = new MemoryStream();
        await response.ResponseStream.CopyToAsync(buffer, cancellationToken);

        return buffer.ToArray();
    }

    public async Task<byte[]> ReadAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        using var response = await s3.GetObjectAsync(_options.DocumentsBucket, objectKey, cancellationToken);

        using var buffer = new MemoryStream();
        await response.ResponseStream.CopyToAsync(buffer, cancellationToken);

        return buffer.ToArray();
    }

    public async Task MoveAsync(string sourceKey, string targetKey, CancellationToken cancellationToken = default)
    {
        await s3.CopyObjectAsync(
            new CopyObjectRequest
            {
                SourceBucket = _options.DocumentsBucket,
                SourceKey = sourceKey,
                DestinationBucket = _options.DocumentsBucket,
                DestinationKey = targetKey
            },
            cancellationToken);

        await s3.DeleteObjectAsync(_options.DocumentsBucket, sourceKey, cancellationToken);
    }

    public async Task DeleteAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        await s3.DeleteObjectAsync(_options.DocumentsBucket, objectKey, cancellationToken);
    }

    public async Task CopyToPublicAsync(
        string objectKey,
        string publicKey,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var request = new CopyObjectRequest
        {
            SourceBucket = _options.DocumentsBucket,
            SourceKey = objectKey,
            DestinationBucket = _options.PublicBucket,
            DestinationKey = publicKey,
            ContentType = contentType,
            MetadataDirective = S3MetadataDirective.REPLACE
        };

        request.Headers.CacheControl = PublicCacheControl;

        await s3.CopyObjectAsync(request, cancellationToken);
    }

    public async Task DeletePublicAsync(string publicKey, CancellationToken cancellationToken = default)
    {
        await s3.DeleteObjectAsync(_options.PublicBucket, publicKey, cancellationToken);
    }

    public string GetPublicUrl(string publicKey) => $"{_options.PublicBaseUrl.TrimEnd('/')}/{publicKey}";
}
