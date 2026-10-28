namespace FourPrep.Modules.Academy.Application.Courses;

using FluentValidation;
using System.Text.RegularExpressions;
using FourPrep.Common.Domain.Localization;
using FourPrep.Common.Application.Extensions;
using FourPrep.Modules.Academy.Domain.Courses;

internal sealed partial class CourseDetailsValidator : AbstractValidator<CourseDetails>
{
    private const int MaxSlugLength = 100;
    private const int MaxNameLength = 200;
    private const int MaxDescriptionLength = 4000;

    public CourseDetailsValidator()
    {
        RuleFor(x => x.Slug)
            .NotEmpty()
            .MaximumLength(MaxSlugLength)
            .Must(slug => string.IsNullOrWhiteSpace(slug) || SlugPattern().IsMatch(slug.Trim()))
            .WithError(CourseErrors.InvalidSlug);

        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .Must(LocalizedTexts.HasEnglish)
            .WithError(CourseErrors.NameRequired)
            .LocalizedText(MaxNameLength);

        RuleFor(x => x.Description).LocalizedText(MaxDescriptionLength);
    }

    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$")]
    private static partial Regex SlugPattern();
}
