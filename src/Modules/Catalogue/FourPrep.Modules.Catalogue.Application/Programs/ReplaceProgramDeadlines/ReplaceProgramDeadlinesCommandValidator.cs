namespace FourPrep.Modules.Catalogue.Application.Programs.ReplaceProgramDeadlines;

using FluentValidation;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Extensions;
using FourPrep.Modules.Catalogue.Domain.Deadlines;
using FourPrep.Modules.Catalogue.Application.Facts;

internal sealed class ReplaceProgramDeadlinesCommandValidator : AbstractValidator<ReplaceProgramDeadlinesCommand>
{
    private const int MaxDeadlines = 20;
    private const int MaxRoundLength = 100;

    public ReplaceProgramDeadlinesCommandValidator(ITimeZoneContext timeZoneContext)
    {
        RuleFor(x => x.Deadlines)
            .NotNull()
            .Must(deadlines => deadlines.Count <= MaxDeadlines)
            .Must(deadlines => deadlines
                .Select(deadline => deadline.Round?.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count() == deadlines.Count)
            .WithError(DeadlineErrors.DuplicateRound);

        RuleForEach(x => x.Deadlines).ChildRules(deadline =>
        {
            deadline.RuleFor(x => x.Round).NotEmpty().MaximumLength(MaxRoundLength);
            deadline.RuleFor(x => x.SourceUrl).SourceUrl();
            deadline.RuleFor(x => x.CheckedOn).CheckedOn(timeZoneContext);
        });
    }
}
