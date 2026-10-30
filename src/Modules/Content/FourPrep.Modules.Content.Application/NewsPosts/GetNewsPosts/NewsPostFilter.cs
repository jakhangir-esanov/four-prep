namespace FourPrep.Modules.Content.Application.NewsPosts.GetNewsPosts;

using FourPrep.Common.Application.Data;
using FourPrep.Common.Domain.Localization;
using FourPrep.Modules.Content.Domain.NewsPosts;

public sealed record NewsPostFilter(
    NewsPostStatus? Status = null,
    string? Tag = null,
    string? Locale = null,
    string? Search = null)
{
    public static readonly NewsPostFilter None = new();

    public string? NormalizedTag => string.IsNullOrWhiteSpace(Tag) ? null : Tag.Trim().ToLowerInvariant();

    public string? NormalizedLocale => Locales.NormalizeOrNull(Locale);

    public string? SearchPattern => LikePatterns.Contains(Search);
}
