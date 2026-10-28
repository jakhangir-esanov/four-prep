namespace FourPrep.Modules.Catalogue.Application.Universities.CreateUniversity;

using FluentValidation;
using FourPrep.Common.Application.Clock;

internal sealed class CreateUniversityCommandValidator : AbstractValidator<CreateUniversityCommand>
{
    public CreateUniversityCommandValidator(ITimeZoneContext timeZoneContext)
    {
        RuleFor(x => x.Details).NotNull().SetValidator(new UniversityDetailsValidator(timeZoneContext));
    }
}
