namespace FourPrep.Modules.Content.Application.NewsPosts.GetNewsPosts;

using FourPrep.Modules.Content.Domain.NewsPosts;

public sealed record NewsPostResponse
{
    public Guid Id { get; init; }
    public string Slug { get; init; } = string.Empty;
    public Dictionary<string, string> Title { get; init; } = [];
    public Dictionary<string, string> Body { get; init; } = [];
    public string? Tag { get; init; }
    public Guid? CoverFileId { get; init; }
    public string? CoverUrl { get; init; }
    public NewsPostStatus Status { get; init; }
    public DateTime? PublishedOnUtc { get; init; }
    public DateTime CreatedOnUtc { get; init; }
    public DateTime UpdatedOnUtc { get; init; }

    internal NewsPostResponse ForPublic() => this with { CoverFileId = null };
}
