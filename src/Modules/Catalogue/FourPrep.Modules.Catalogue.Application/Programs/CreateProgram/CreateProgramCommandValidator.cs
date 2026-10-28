namespace FourPrep.Modules.Catalogue.Application.Programs.CreateProgram;

using FluentValidation;
using FourPrep.Common.Application.Clock;

internal sealed class CreateProgramCommandValidator : AbstractValidator<CreateProgramCommand>
{
    public CreateProgramCommandValidator(ITimeZoneContext timeZoneContext)
    {
        RuleFor(x => x.Details).NotNull().SetValidator(new ProgramDetailsValidator(timeZoneContext));
    }
}
