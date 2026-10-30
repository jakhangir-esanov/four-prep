namespace FourPrep.Modules.Content.Presentation.NewsPosts.Requests;

using FourPrep.Modules.Content.Application.NewsPosts;

internal sealed record NewsPostRequest
{
    public string? Slug { get; init; }
    public Dictionary<string, string>? Title { get; init; }
    public Dictionary<string, string>? Body { get; init; }
    public string? Tag { get; init; }

    public NewsPostDetails ToDetails() => new(Slug ?? string.Empty, Title, Body, Tag);
}
