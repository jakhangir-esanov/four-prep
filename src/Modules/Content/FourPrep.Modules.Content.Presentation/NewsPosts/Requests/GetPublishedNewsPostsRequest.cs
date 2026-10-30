namespace FourPrep.Modules.Content.Presentation.NewsPosts.Requests;

internal sealed record GetPublishedNewsPostsRequest
{
    public int? First { get; init; }
    public int? Rows { get; init; }
    public string? SortField { get; init; }
    public int? SortOrder { get; init; }

    public string? Tag { get; init; }
    public string? Locale { get; init; }
}
