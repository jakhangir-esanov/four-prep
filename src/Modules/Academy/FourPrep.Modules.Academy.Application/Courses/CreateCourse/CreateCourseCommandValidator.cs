namespace FourPrep.Modules.Academy.Application.Courses.CreateCourse;

using FluentValidation;

internal sealed class CreateCourseCommandValidator : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseCommandValidator()
    {
        RuleFor(x => x.Details).NotNull().SetValidator(new CourseDetailsValidator());
    }
}
