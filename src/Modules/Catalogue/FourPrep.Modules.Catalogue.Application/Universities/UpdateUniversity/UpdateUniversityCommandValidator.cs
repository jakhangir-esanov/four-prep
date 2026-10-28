namespace FourPrep.Modules.Catalogue.Application.Universities.UpdateUniversity;

using FluentValidation;
using FourPrep.Common.Application.Clock;

internal sealed class UpdateUniversityCommandValidator : AbstractValidator<UpdateUniversityCommand>
{
    public UpdateUniversityCommandValidator(ITimeZoneContext timeZoneContext)
    {
        RuleFor(x => x.Details).NotNull().SetValidator(new UniversityDetailsValidator(timeZoneContext));
    }
}
