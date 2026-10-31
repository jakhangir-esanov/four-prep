namespace FourPrep.Modules.Admission.Application.AcademyApplications.DeclineAcademyApplication;

using FluentValidation;
using FourPrep.Common.Application.Extensions;
using FourPrep.Modules.Admission.Domain.AcademyApplications;

internal sealed class DeclineAcademyApplicationCommandValidator : AbstractValidator<DeclineAcademyApplicationCommand>
{
    private const int MaxReasonLength = 500;

    public DeclineAcademyApplicationCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Reason)
            .Must(reason => !string.IsNullOrWhiteSpace(reason) && reason.Trim().Length <= MaxReasonLength)
            .WithError(AcademyApplicationErrors.DeclineReasonRequired);
    }
}
