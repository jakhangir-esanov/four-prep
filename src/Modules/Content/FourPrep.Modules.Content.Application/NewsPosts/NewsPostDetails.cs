namespace FourPrep.Modules.Content.Application.NewsPosts;

using FourPrep.Common.Domain.Localization;
using FourPrep.Modules.Content.Domain.NewsPosts;

public sealed record NewsPostDetails(
    string Slug,
    IReadOnlyDictionary<string, string>? Title,
    IReadOnlyDictionary<string, string>? Body,
    string? Tag)
{
    public string NormalizedSlug => Slug.Trim();

    public string? NormalizedTag => string.IsNullOrWhiteSpace(Tag) ? null : Tag.Trim();

    internal NewsPost ToNewsPost(DateTime utcNow) =>
        NewsPost.Create(
            NormalizedSlug,
            LocalizedTexts.Normalize(Title),
            LocalizedTexts.Normalize(Body),
            NormalizedTag,
            utcNow);

    internal void ApplyTo(NewsPost newsPost, DateTime utcNow) =>
        newsPost.Update(
            NormalizedSlug,
            LocalizedTexts.Normalize(Title),
            LocalizedTexts.Normalize(Body),
            NormalizedTag,
            utcNow);
}
