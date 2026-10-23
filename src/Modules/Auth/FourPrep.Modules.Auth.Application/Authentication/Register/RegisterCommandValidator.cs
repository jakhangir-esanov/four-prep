namespace FourPrep.Modules.Auth.Application.Authentication.Register;

using FluentValidation;
using FourPrep.Modules.Auth.Domain.Users;
using FourPrep.Common.Domain.Authorization;
using FourPrep.Common.Application.Extensions;

internal sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    private const string UsernamePattern = "^[A-Za-z0-9._@-]+$";

    public RegisterCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .MaximumLength(128)
            .Matches(UsernamePattern)
            .WithError(UserErrors.InvalidUsername);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(x => x.PhoneNumber).MaximumLength(32);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
        RuleFor(x => x.Role)
            .Must(role => role is Roles.Student or Roles.Parent)
            .WithError(UserErrors.InvalidRole);
    }
}
