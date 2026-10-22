namespace FourPrep.Modules.Auth.Application.Users.CreateUser;

using FluentValidation;
using FourPrep.Common.Domain.Authorization;
using FourPrep.Modules.Auth.Domain.Users;
using FourPrep.Common.Application.Extensions;

internal sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    private const string UsernamePattern = "^[A-Za-z0-9._@-]+$";

    public CreateUserCommandValidator()
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
            .NotEmpty()
            .Must(role => Roles.All.Contains(role))
            .WithError(UserErrors.InvalidRole);
    }
}
