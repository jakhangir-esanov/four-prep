namespace FourPrep.Modules.Content.Application.NewsPosts;

using FluentValidation;
using System.Text.RegularExpressions;
using FourPrep.Common.Domain.Localization;
using FourPrep.Common.Application.Extensions;
using FourPrep.Modules.Content.Domain.NewsPosts;

internal sealed partial class NewsPostDetailsValidator : AbstractValidator<NewsPostDetails>
{
    private const int MaxSlugLength = 150;
    private const int MaxTitleLength = 200;
    private const int MaxBodyLength = 20000;
    private const int MaxTagLength = 50;

    public NewsPostDetailsValidator()
    {
        RuleFor(x => x.Slug)
            .NotEmpty()
            .MaximumLength(MaxSlugLength)
            .Must(slug => string.IsNullOrWhiteSpace(slug) || SlugPattern().IsMatch(slug.Trim()))
            .WithError(NewsPostErrors.InvalidSlug);

        RuleFor(x => x.Title)
            .Cascade(CascadeMode.Stop)
            .Must(title => LocalizedTexts.Normalize(title).Count > 0)
            .WithError(NewsPostErrors.TitleRequired)
            .LocalizedTextInAnyLanguage(MaxTitleLength);

        RuleFor(x => x.Body).LocalizedTextInAnyLanguage(MaxBodyLength);

        RuleFor(x => x.Body)
            .Must((details, body) => HaveSameLanguages(details.Title, body))
            .When(details => LocalizedTexts.Normalize(details.Title).Count > 0)
            .WithError(NewsPostErrors.TextLanguagesMismatch);

        RuleFor(x => x.Tag)
            .MaximumLength(MaxTagLength)
            .Must(tag => string.IsNullOrWhiteSpace(tag) || SlugPattern().IsMatch(tag.Trim()))
            .WithError(NewsPostErrors.InvalidTag);
    }

    private static bool HaveSameLanguages(
        IReadOnlyDictionary<string, string>? title,
        IReadOnlyDictionary<string, string>? body) =>
        LocalizedTexts.Normalize(title).Keys.ToHashSet().SetEquals(LocalizedTexts.Normalize(body).Keys);

    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$")]
    private static partial Regex SlugPattern();
}
