namespace FourPrep.Modules.Student.Application.StudentParents.LinkStudent;

using FluentValidation;
using FourPrep.Modules.Student.Application.Abstractions.LinkCodes;

internal sealed class LinkStudentCommandValidator : AbstractValidator<LinkStudentCommand>
{
    public LinkStudentCommandValidator()
    {
        RuleFor(x => x.LinkCode)
            .NotEmpty()
            .Must(code => code is not null && LinkCodeFormat.Normalize(code).Length == LinkCodeFormat.Length);
    }
}
