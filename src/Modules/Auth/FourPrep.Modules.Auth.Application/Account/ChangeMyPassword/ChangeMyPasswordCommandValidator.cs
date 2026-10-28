namespace FourPrep.Modules.Auth.Application.Account.ChangeMyPassword;

using FluentValidation;
using FourPrep.Modules.Auth.Domain.Users;
using FourPrep.Common.Application.Extensions;

internal sealed class ChangeMyPasswordCommandValidator : AbstractValidator<ChangeMyPasswordCommand>
{
    private const int MinPasswordLength = 8;
    private const int MaxPasswordLength = 128;

    public ChangeMyPasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty().MaximumLength(MaxPasswordLength);
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(MinPasswordLength).MaximumLength(MaxPasswordLength);
        RuleFor(x => x.NewPassword)
            .NotEqual(x => x.CurrentPassword)
            .WithError(UserErrors.PasswordUnchanged);
    }
}
