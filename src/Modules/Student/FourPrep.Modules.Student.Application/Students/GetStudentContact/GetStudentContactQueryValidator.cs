namespace FourPrep.Modules.Student.Application.Students.GetStudentContact;

using FluentValidation;

internal sealed class GetStudentContactQueryValidator : AbstractValidator<GetStudentContactQuery>
{
    public GetStudentContactQueryValidator()
    {
        RuleFor(x => x.StudentUserId).NotEmpty();
    }
}
