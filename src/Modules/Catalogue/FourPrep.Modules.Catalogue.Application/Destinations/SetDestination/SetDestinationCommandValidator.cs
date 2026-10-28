namespace FourPrep.Modules.Catalogue.Application.Destinations.SetDestination;

using FluentValidation;
using FourPrep.Common.Application.Extensions;

internal sealed class SetDestinationCommandValidator : AbstractValidator<SetDestinationCommand>
{
    private const int MaxVisaTypeLength = 100;

    public SetDestinationCommandValidator()
    {
        RuleFor(x => x.CountryCode).NotEmpty().CountryCode();
        RuleFor(x => x.VisaType).NotEmpty().MaximumLength(MaxVisaTypeLength);
    }
}
