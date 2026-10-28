namespace FourPrep.Modules.Academy.Application.Courses.UpdateCourse;

using FluentValidation;

internal sealed class UpdateCourseCommandValidator : AbstractValidator<UpdateCourseCommand>
{
    public UpdateCourseCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Details).NotNull().SetValidator(new CourseDetailsValidator());
    }
}
