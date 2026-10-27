namespace FourPrep.Modules.Student.Application.Students.CreateStudent;

using FluentValidation;
using FourPrep.Common.Application.Extensions;

internal sealed class CreateStudentCommandValidator : AbstractValidator<CreateStudentCommand>
{
    public CreateStudentCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.UserName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Country).CountryCode();
    }
}
