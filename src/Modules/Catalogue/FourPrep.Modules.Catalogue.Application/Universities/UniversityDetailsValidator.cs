namespace FourPrep.Modules.Catalogue.Application.Universities;

using FluentValidation;
using System.Text.RegularExpressions;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Extensions;
using FourPrep.Modules.Catalogue.Application.Facts;
using FourPrep.Modules.Catalogue.Domain.Universities;

internal sealed partial class UniversityDetailsValidator : AbstractValidator<UniversityDetails>
{
    private const int MaxSlugLength = 100;
    private const int MaxNameLength = 200;
    private const int MaxCityLength = 100;
    private const int MaxRankingSourceLength = 200;
    private const int MaxDescriptionLength = 4000;
    private const decimal MaxPercentage = 100;

    public UniversityDetailsValidator(ITimeZoneContext timeZoneContext)
    {
        RuleFor(x => x.Slug)
            .NotEmpty()
            .MaximumLength(MaxSlugLength)
            .Must(slug => string.IsNullOrWhiteSpace(slug) || SlugPattern().IsMatch(slug.Trim()))
            .WithError(UniversityErrors.InvalidSlug);

        RuleFor(x => x.Name).NotEmpty().MaximumLength(MaxNameLength);
        RuleFor(x => x.CountryCode).NotEmpty().CountryCode();
        RuleFor(x => x.City).MaximumLength(MaxCityLength);

        RuleFor(x => x.Website)
            .MaximumLength(FactRules.MaxUrlLength)
            .Must(FactRules.IsWebAddress)
            .When(x => !string.IsNullOrWhiteSpace(x.Website))
            .WithError(UniversityErrors.InvalidWebsite);

        RuleFor(x => x.RankingSource).MaximumLength(MaxRankingSourceLength);
        RuleFor(x => x.Ranking).GreaterThan(0);
        RuleFor(x => x.RankingSource)
            .NotEmpty()
            .When(x => x.Ranking is not null)
            .WithError(UniversityErrors.RankingSourceRequired);

        RuleFor(x => x.AcceptanceRate)
            .InclusiveBetween(0, MaxPercentage)
            .WithError(UniversityErrors.InvalidAcceptanceRate);

        RuleFor(x => x.Description).LocalizedText(MaxDescriptionLength);
        RuleFor(x => x.SourceUrl).SourceUrl();
        RuleFor(x => x.CheckedOn).CheckedOn(timeZoneContext);
    }

    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$")]
    private static partial Regex SlugPattern();
}
