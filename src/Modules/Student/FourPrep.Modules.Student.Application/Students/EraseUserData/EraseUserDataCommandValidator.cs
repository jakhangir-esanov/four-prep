namespace FourPrep.Modules.Student.Application.Students.EraseUserData;

using FluentValidation;

internal sealed class EraseUserDataCommandValidator : AbstractValidator<EraseUserDataCommand>
{
    public EraseUserDataCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
