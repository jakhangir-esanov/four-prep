namespace FourPrep.Modules.Auth.Application.Users.UpdateUserContact;

using FluentValidation;

internal sealed class UpdateUserContactCommandValidator : AbstractValidator<UpdateUserContactCommand>
{
    public UpdateUserContactCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(x => x.PhoneNumber).MaximumLength(32);
    }
}
