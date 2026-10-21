namespace FourPrep.Common.Infrastructure.Files;

using System.Text;
using System.Buffers.Text;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using FourPrep.Common.Application.Files;
using FourPrep.Common.Application.Clock;

internal sealed class HmacFileUrlSigner : IFileUrlSigner
{
    private const int SigningKeyBytes = 32;

    private readonly byte[] signingKey;
    private readonly FileLinkOptions options;
    private readonly IDateTimeProvider dateTimeProvider;

    public HmacFileUrlSigner(IOptions<FileLinkOptions> options, IDateTimeProvider dateTimeProvider)
    {
        this.options = options.Value;
        this.dateTimeProvider = dateTimeProvider;

        signingKey = string.IsNullOrWhiteSpace(this.options.SigningKey)
            ? RandomNumberGenerator.GetBytes(SigningKeyBytes)
            : Encoding.UTF8.GetBytes(this.options.SigningKey);
    }

    public bool VideosRequireSignature => options.RequireSignedVideos;

    public string ContentUrl(Guid fileId)
    {
        long expires = CurrentExpiry();

        return $"/files/{fileId}/content?expires={expires}&signature={Sign(fileId, expires)}";
    }

    public string StreamUrl(Guid fileId) => StreamFileUrl(fileId, "master.m3u8");

    public string PosterUrl(Guid fileId) => StreamFileUrl(fileId, "poster.jpg");

    public bool IsValid(Guid fileId, long? expires, string? signature)
    {
        if (expires is null || string.IsNullOrEmpty(signature))
        {
            return false;
        }

        long now = new DateTimeOffset(DateTime.SpecifyKind(dateTimeProvider.UtcNow, DateTimeKind.Utc)).ToUnixTimeSeconds();
        if (expires.Value <= now)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(
            Encoding.ASCII.GetBytes(Sign(fileId, expires.Value)),
            Encoding.ASCII.GetBytes(signature));
    }

    private string StreamFileUrl(Guid fileId, string fileName)
    {
        long expires = CurrentExpiry();

        return $"/streams/{fileId}/{expires}.{Sign(fileId, expires)}/{fileName}";
    }

    private long CurrentExpiry()
    {
        var utcNow = DateTime.SpecifyKind(dateTimeProvider.UtcNow, DateTimeKind.Utc);
        var startOfHour = new DateTimeOffset(utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour, 0, 0, TimeSpan.Zero);

        return startOfHour.AddHours(options.LifetimeHours + 1).ToUnixTimeSeconds();
    }

    private string Sign(Guid fileId, long expires)
    {
        var payload = Encoding.UTF8.GetBytes($"{fileId:N}.{expires}");

        return Base64Url.EncodeToString(HMACSHA256.HashData(signingKey, payload));
    }
}
