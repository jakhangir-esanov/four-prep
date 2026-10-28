namespace FourPrep.Modules.Catalogue.Application.Universities.ReplaceUniversityCosts;

using FluentValidation;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Extensions;
using FourPrep.Modules.Catalogue.Domain.Costs;
using FourPrep.Modules.Catalogue.Application.Facts;

internal sealed class ReplaceUniversityCostsCommandValidator : AbstractValidator<ReplaceUniversityCostsCommand>
{
    private const int MaxCosts = 10;
    private const int FirstAcademicYear = 2000;
    private const int LastAcademicYear = 2100;

    public ReplaceUniversityCostsCommandValidator(ITimeZoneContext timeZoneContext)
    {
        RuleFor(x => x.Costs)
            .NotNull()
            .Must(costs => costs.Count <= MaxCosts)
            .Must(costs => costs.Select(cost => cost.AcademicYear).Distinct().Count() == costs.Count)
            .WithError(CostErrors.DuplicateAcademicYear);

        RuleForEach(x => x.Costs).ChildRules(cost =>
        {
            cost.RuleFor(x => x.AcademicYear)
                .InclusiveBetween(FirstAcademicYear, LastAcademicYear)
                .WithError(CostErrors.InvalidAcademicYear);
            cost.RuleFor(x => x.TuitionMinor).GreaterThanOrEqualTo(0).WithError(CostErrors.InvalidAmount);
            cost.RuleFor(x => x.LivingMinor).GreaterThanOrEqualTo(0).WithError(CostErrors.InvalidAmount);
            cost.RuleFor(x => x.Currency).NotEmpty().CurrencyCode();
            cost.RuleFor(x => x.SourceUrl).SourceUrl();
            cost.RuleFor(x => x.CheckedOn).CheckedOn(timeZoneContext);
        });
    }
}
