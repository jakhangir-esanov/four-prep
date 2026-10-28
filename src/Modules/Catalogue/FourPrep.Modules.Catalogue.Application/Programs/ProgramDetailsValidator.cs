namespace FourPrep.Modules.Catalogue.Application.Programs;

using FluentValidation;
using System.Text.RegularExpressions;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Extensions;
using FourPrep.Modules.Catalogue.Domain.Programs;
using FourPrep.Modules.Catalogue.Application.Facts;

internal sealed partial class ProgramDetailsValidator : AbstractValidator<ProgramDetails>
{
    private const int MaxNameLength = 200;
    private const int MaxFieldLength = 100;
    private const int MaxDurationMonths = 120;

    public ProgramDetailsValidator(ITimeZoneContext timeZoneContext)
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(MaxNameLength);
        RuleFor(x => x.DegreeLevel).IsInEnum();
        RuleFor(x => x.Field).NotEmpty().MaximumLength(MaxFieldLength);

        RuleFor(x => x.Language)
            .NotEmpty()
            .Must(language => string.IsNullOrWhiteSpace(language) || LanguageCodePattern().IsMatch(language.Trim()))
            .WithError(ProgramErrors.InvalidLanguage);

        RuleFor(x => x.DurationMonths)
            .InclusiveBetween(1, MaxDurationMonths)
            .WithError(ProgramErrors.InvalidDuration);

        RuleFor(x => x.SourceUrl).SourceUrl();
        RuleFor(x => x.CheckedOn).CheckedOn(timeZoneContext);
    }

    [GeneratedRegex("^[a-zA-Z]{2}$")]
    private static partial Regex LanguageCodePattern();
}
