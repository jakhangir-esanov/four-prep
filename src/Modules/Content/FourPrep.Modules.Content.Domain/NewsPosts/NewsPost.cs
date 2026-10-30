namespace FourPrep.Modules.Content.Domain.NewsPosts;

using FourPrep.Common.Domain;

public sealed class NewsPost
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public Dictionary<string, string> Title { get; set; } = [];
    public Dictionary<string, string> Body { get; set; } = [];
    public string? Tag { get; set; }
    public Guid? CoverFileId { get; set; }
    public string? CoverUrl { get; set; }
    public NewsPostStatus Status { get; set; }
    public DateTimeOffset? PublishedOnUtc { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset UpdatedOnUtc { get; set; }

    public static NewsPost Create(
        string slug,
        Dictionary<string, string> title,
        Dictionary<string, string> body,
        string? tag,
        DateTime utcNow)
    {
        var newsPost = new NewsPost
        {
            Id = Guid.NewGuid(),
            Status = NewsPostStatus.Draft,
            CreatedOnUtc = utcNow
        };

        newsPost.Update(slug, title, body, tag, utcNow);

        return newsPost;
    }

    public void Update(
        string slug,
        Dictionary<string, string> title,
        Dictionary<string, string> body,
        string? tag,
        DateTime utcNow)
    {
        Slug = slug;
        Title = title;
        Body = body;
        Tag = tag;
        UpdatedOnUtc = utcNow;
    }

    public Result Publish(DateTime utcNow)
    {
        if (Status == NewsPostStatus.Published)
        {
            return Result.Failure(NewsPostErrors.AlreadyPublished);
        }

        Status = NewsPostStatus.Published;
        PublishedOnUtc ??= utcNow;
        UpdatedOnUtc = utcNow;

        return Result.Success();
    }

    public Result Unpublish(DateTime utcNow)
    {
        if (Status != NewsPostStatus.Published)
        {
            return Result.Failure(NewsPostErrors.NotPublished);
        }

        Status = NewsPostStatus.Draft;
        UpdatedOnUtc = utcNow;

        return Result.Success();
    }

    public void SetCover(Guid fileId, string url, DateTime utcNow)
    {
        CoverFileId = fileId;
        CoverUrl = url;
        UpdatedOnUtc = utcNow;
    }

    public void RemoveCover(DateTime utcNow)
    {
        CoverFileId = null;
        CoverUrl = null;
        UpdatedOnUtc = utcNow;
    }
}
