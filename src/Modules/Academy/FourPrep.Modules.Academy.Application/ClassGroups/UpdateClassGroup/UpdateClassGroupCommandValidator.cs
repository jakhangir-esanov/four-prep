namespace FourPrep.Modules.Academy.Application.ClassGroups.UpdateClassGroup;

using FluentValidation;

internal sealed class UpdateClassGroupCommandValidator : AbstractValidator<UpdateClassGroupCommand>
{
    public UpdateClassGroupCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Details).NotNull().SetValidator(new ClassGroupDetailsValidator());
    }
}
