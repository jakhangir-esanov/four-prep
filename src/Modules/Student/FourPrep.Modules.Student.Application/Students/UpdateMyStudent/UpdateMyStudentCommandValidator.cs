namespace FourPrep.Modules.Student.Application.Students.UpdateMyStudent;

using FluentValidation;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Extensions;
using FourPrep.Modules.Student.Domain.Students;
using FourPrep.Modules.Student.Domain.ExamGoals;

internal sealed class UpdateMyStudentCommandValidator : AbstractValidator<UpdateMyStudentCommand>
{
    private const int MinAge = 5;
    private const int MaxAge = 100;
    private const int MaxExamGoals = 4;

    public UpdateMyStudentCommandValidator(ITimeZoneContext timeZoneContext)
    {
        RuleFor(x => x.FirstName).MaximumLength(100);
        RuleFor(x => x.LastName).MaximumLength(100);
        RuleFor(x => x.School).MaximumLength(200);

        RuleFor(x => x.Grade)
            .InclusiveBetween(1, 11)
            .WithError(StudentErrors.InvalidGrade);

        RuleFor(x => x.Region).IsInEnum();

        RuleFor(x => x.DateOfBirth)
            .Must(dateOfBirth => IsPlausibleBirthDate(dateOfBirth!.Value, timeZoneContext.Today()))
            .When(x => x.DateOfBirth is not null)
            .WithError(StudentErrors.InvalidDateOfBirth);

        RuleFor(x => x.ExamGoals)
            .NotNull()
            .Must(goals => goals.Count <= MaxExamGoals)
            .Must(goals => goals.Select(goal => goal.Exam).Distinct().Count() == goals.Count)
            .WithError(ExamGoalErrors.DuplicateExam);

        RuleForEach(x => x.ExamGoals).ChildRules(goal =>
        {
            goal.RuleFor(x => x.Exam).IsInEnum();
            goal.RuleFor(x => x)
                .Must(x => ExamScores.IsValid(x.Exam, x.TargetScore))
                .When(x => Enum.IsDefined(x.Exam))
                .WithError(ExamGoalErrors.InvalidTargetScore);
        });
    }

    private static bool IsPlausibleBirthDate(DateOnly dateOfBirth, DateOnly today) =>
        dateOfBirth <= today.AddYears(-MinAge) && dateOfBirth >= today.AddYears(-MaxAge);
}
