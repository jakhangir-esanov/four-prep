namespace FourPrep.Modules.Academy.Application.ClassGroups.AssignTeacher;

using FluentValidation;

internal sealed class AssignTeacherCommandValidator : AbstractValidator<AssignTeacherCommand>
{
    private const int MaxTeacherNameLength = 200;

    public AssignTeacherCommandValidator()
    {
        RuleFor(x => x.ClassGroupId).NotEmpty();
        RuleFor(x => x.TeacherUserId).NotEmpty();
        RuleFor(x => x.TeacherName).NotEmpty().MaximumLength(MaxTeacherNameLength);
    }
}
