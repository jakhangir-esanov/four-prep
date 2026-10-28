namespace FourPrep.Modules.Auth.Application.Users.DeleteUser;

using FluentValidation;

internal sealed class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
