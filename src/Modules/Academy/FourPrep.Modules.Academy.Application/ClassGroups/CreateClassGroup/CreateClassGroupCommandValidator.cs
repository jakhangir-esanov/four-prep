namespace FourPrep.Modules.Academy.Application.ClassGroups.CreateClassGroup;

using FluentValidation;

internal sealed class CreateClassGroupCommandValidator : AbstractValidator<CreateClassGroupCommand>
{
    public CreateClassGroupCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.Details).NotNull().SetValidator(new ClassGroupDetailsValidator());
    }
}
