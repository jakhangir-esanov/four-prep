namespace FourPrep.Modules.Auth.Application.Account.DeleteMyAccount;

using FluentValidation;

internal sealed class DeleteMyAccountCommandValidator : AbstractValidator<DeleteMyAccountCommand>
{
    private const int MaxPasswordLength = 128;

    public DeleteMyAccountCommandValidator()
    {
        RuleFor(x => x.Password).MaximumLength(MaxPasswordLength);
    }
}
