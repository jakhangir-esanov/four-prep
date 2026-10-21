namespace FourPrep.Common.UnitTests.Files;

using Microsoft.Extensions.Options;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Infrastructure.Files;

public class HmacFileUrlSignerTests
{
    private static readonly Guid FileId = Guid.NewGuid();
    private static readonly DateTime Noon = new(2026, 10, 17, 12, 20, 0, DateTimeKind.Utc);

    private static HmacFileUrlSigner Signer(DateTime utcNow, string key = "test-signing-key", bool required = true) =>
        new(
            Options.Create(new FileLinkOptions { SigningKey = key, LifetimeHours = 6, RequireSignedVideos = required }),
            new FixedClock(utcNow));

    private static (long Expires, string Signature) Parse(string url)
    {
        var query = url[(url.IndexOf('?') + 1)..]
            .Split('&')
            .Select(part => part.Split('=', 2))
            .ToDictionary(pair => pair[0], pair => pair[1]);

        return (long.Parse(query["expires"]), query["signature"]);
    }

    [Fact]
    public void ContentUrl_PointsAtTheFileAndLastsUntilTheHourAfterTheLifetime()
    {
        var url = Signer(Noon).ContentUrl(FileId);

        Assert.StartsWith($"/files/{FileId}/content?expires=", url);
        var (expires, _) = Parse(url);
        Assert.Equal(new DateTimeOffset(2026, 10, 17, 19, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds(), expires);
    }

    [Fact]
    public void ContentUrl_IsTheSameWithinAnHourSoPlayersCanCache()
    {
        Assert.Equal(Signer(Noon).ContentUrl(FileId), Signer(Noon.AddMinutes(30)).ContentUrl(FileId));
    }

    [Fact]
    public void IsValid_AcceptsItsOwnLink()
    {
        var (expires, signature) = Parse(Signer(Noon).ContentUrl(FileId));

        Assert.True(Signer(Noon.AddHours(5)).IsValid(FileId, expires, signature));
    }

    [Fact]
    public void IsValid_RejectsAnExpiredLink()
    {
        var (expires, signature) = Parse(Signer(Noon).ContentUrl(FileId));

        Assert.False(Signer(Noon.AddHours(7)).IsValid(FileId, expires, signature));
    }

    [Fact]
    public void IsValid_RejectsALinkCopiedToAnotherFileOrWithAMovedExpiry()
    {
        var (expires, signature) = Parse(Signer(Noon).ContentUrl(FileId));

        Assert.False(Signer(Noon).IsValid(Guid.NewGuid(), expires, signature));
        Assert.False(Signer(Noon).IsValid(FileId, expires + 3600, signature));
    }

    [Fact]
    public void IsValid_RejectsALinkSignedWithAnotherKeyOrMissingParts()
    {
        var (expires, signature) = Parse(Signer(Noon, key: "other-key").ContentUrl(FileId));

        Assert.False(Signer(Noon).IsValid(FileId, expires, signature));
        Assert.False(Signer(Noon).IsValid(FileId, null, signature));
        Assert.False(Signer(Noon).IsValid(FileId, expires, null));
    }

    [Fact]
    public void StreamUrl_CarriesTheSignatureInThePathSoSegmentLinksInheritIt()
    {
        var url = Signer(Noon).StreamUrl(FileId);

        var parts = url.Split('/');
        Assert.Equal(["", "streams", FileId.ToString(), parts[3], "master.m3u8"], parts);

        var token = parts[3].Split('.', 2);
        Assert.Equal(new DateTimeOffset(2026, 10, 17, 19, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds(), long.Parse(token[0]));
        Assert.True(Signer(Noon.AddHours(5)).IsValid(FileId, long.Parse(token[0]), token[1]));
    }

    [Fact]
    public void PosterUrl_SitsNextToThePlaylistWithTheSameSignature()
    {
        var stream = Signer(Noon).StreamUrl(FileId);
        var poster = Signer(Noon).PosterUrl(FileId);

        Assert.Equal(stream.Replace("master.m3u8", "poster.jpg"), poster);
    }

    [Fact]
    public void StreamUrl_SignsTheSameWayAsTheContentLink()
    {
        var (_, contentSignature) = Parse(Signer(Noon).ContentUrl(FileId));
        var streamToken = Signer(Noon).StreamUrl(FileId).Split('/')[3];

        Assert.EndsWith($".{contentSignature}", streamToken);
    }

    [Fact]
    public void VideosRequireSignature_FollowsTheFlag()
    {
        Assert.True(Signer(Noon).VideosRequireSignature);
        Assert.False(Signer(Noon, required: false).VideosRequireSignature);
    }

    private sealed class FixedClock(DateTime utcNow) : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = utcNow;
    }
}
