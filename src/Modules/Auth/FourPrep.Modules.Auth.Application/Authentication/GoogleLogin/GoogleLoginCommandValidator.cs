namespace FourPrep.Modules.Auth.Application.Authentication.GoogleLogin;

using FluentValidation;
using FourPrep.Modules.Auth.Domain.Users;
using FourPrep.Common.Domain.Authorization;
using FourPrep.Common.Application.Extensions;

internal sealed class GoogleLoginCommandValidator : AbstractValidator<GoogleLoginCommand>
{
    public GoogleLoginCommandValidator()
    {
        RuleFor(x => x.IdToken).NotEmpty().MaximumLength(8192);
        RuleFor(x => x.Role)
            .Must(role => role is null or Roles.Student or Roles.Parent)
            .WithError(UserErrors.InvalidRole);
        RuleFor(x => x.Country).CountryCode();
        RuleFor(x => x.TimeZone).TimeZoneId();
        RuleFor(x => x.Locale).Locale();
    }
}
