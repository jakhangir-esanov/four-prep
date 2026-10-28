namespace FourPrep.Modules.Academy.Application.ClassGroups.ChangeClassGroupStatus;

using FluentValidation;

internal sealed class ChangeClassGroupStatusCommandValidator : AbstractValidator<ChangeClassGroupStatusCommand>
{
    public ChangeClassGroupStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();
    }
}
