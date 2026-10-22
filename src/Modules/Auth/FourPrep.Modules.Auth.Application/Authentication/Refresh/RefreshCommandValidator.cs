namespace FourPrep.Modules.Auth.Application.Authentication.Refresh;

using FluentValidation;

internal sealed class RefreshCommandValidator : AbstractValidator<RefreshCommand>
{
    public RefreshCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
