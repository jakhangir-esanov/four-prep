namespace FourPrep.Modules.Catalogue.Application.Universities.ReplaceUniversityScholarships;

using FluentValidation;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Extensions;
using FourPrep.Modules.Catalogue.Application.Facts;

internal sealed class ReplaceUniversityScholarshipsCommandValidator
    : AbstractValidator<ReplaceUniversityScholarshipsCommand>
{
    private const int MaxScholarships = 50;
    private const int MaxNameLength = 200;
    private const int MaxTextLength = 1000;

    public ReplaceUniversityScholarshipsCommandValidator(ITimeZoneContext timeZoneContext)
    {
        RuleFor(x => x.Scholarships)
            .NotNull()
            .Must(scholarships => scholarships.Count <= MaxScholarships);

        RuleForEach(x => x.Scholarships).ChildRules(scholarship =>
        {
            scholarship.RuleFor(x => x.Name).NotEmpty().MaximumLength(MaxNameLength);
            scholarship.RuleFor(x => x.Kind).IsInEnum();
            scholarship.RuleFor(x => x.Amount).LocalizedText(MaxTextLength);
            scholarship.RuleFor(x => x.Eligibility).LocalizedText(MaxTextLength);
            scholarship.RuleFor(x => x.SourceUrl).SourceUrl();
            scholarship.RuleFor(x => x.CheckedOn).CheckedOn(timeZoneContext);
        });
    }
}
