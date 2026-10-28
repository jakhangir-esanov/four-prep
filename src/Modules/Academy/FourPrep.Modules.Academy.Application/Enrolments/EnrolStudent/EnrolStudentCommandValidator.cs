namespace FourPrep.Modules.Academy.Application.Enrolments.EnrolStudent;

using FluentValidation;

internal sealed class EnrolStudentCommandValidator : AbstractValidator<EnrolStudentCommand>
{
    public EnrolStudentCommandValidator()
    {
        RuleFor(x => x.ClassGroupId).NotEmpty();
        RuleFor(x => x.StudentUserId).NotEmpty();
    }
}
