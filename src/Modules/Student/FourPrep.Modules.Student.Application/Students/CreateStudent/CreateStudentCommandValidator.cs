namespace FourPrep.Modules.Student.Application.Students.CreateStudent;

using FluentValidation;

internal sealed class CreateStudentCommandValidator : AbstractValidator<CreateStudentCommand>
{
    public CreateStudentCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.UserName).NotEmpty().MaximumLength(256);
    }
}
