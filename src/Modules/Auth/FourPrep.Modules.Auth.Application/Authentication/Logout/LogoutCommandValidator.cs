namespace FourPrep.Modules.Auth.Application.Authentication.Logout;

using FluentValidation;

internal sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
