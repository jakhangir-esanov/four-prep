namespace FourPrep.Modules.Student.Application.Students.AssignMentor;

using FluentValidation;

internal sealed class AssignMentorCommandValidator : AbstractValidator<AssignMentorCommand>
{
    public AssignMentorCommandValidator()
    {
        RuleFor(x => x.StudentId).NotEmpty();
        RuleFor(x => x.MentorUserId).NotEmpty();
    }
}
