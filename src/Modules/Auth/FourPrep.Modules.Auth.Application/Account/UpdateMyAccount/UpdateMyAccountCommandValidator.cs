namespace FourPrep.Modules.Auth.Application.Account.UpdateMyAccount;

using FluentValidation;
using FourPrep.Common.Application.Extensions;

internal sealed class UpdateMyAccountCommandValidator : AbstractValidator<UpdateMyAccountCommand>
{
    public UpdateMyAccountCommandValidator()
    {
        RuleFor(x => x.PhoneNumber).PhoneNumber();
        RuleFor(x => x.Country).NotEmpty().CountryCode();
        RuleFor(x => x.TimeZone).TimeZoneId();
        RuleFor(x => x.Locale).Locale();
    }
}
