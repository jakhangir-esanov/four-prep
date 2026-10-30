namespace FourPrep.Modules.Content.Presentation.NewsPosts.Requests;

using FourPrep.Modules.Content.Domain.NewsPosts;

internal sealed record GetNewsPostsRequest
{
    public int? First { get; init; }
    public int? Rows { get; init; }
    public string? SortField { get; init; }
    public int? SortOrder { get; init; }

    public NewsPostStatus? Status { get; init; }
    public string? Tag { get; init; }
    public string? Locale { get; init; }
    public string? Search { get; init; }
}
