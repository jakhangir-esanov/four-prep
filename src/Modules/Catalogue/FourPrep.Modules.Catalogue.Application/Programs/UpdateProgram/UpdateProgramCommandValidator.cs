namespace FourPrep.Modules.Catalogue.Application.Programs.UpdateProgram;

using FluentValidation;
using FourPrep.Common.Application.Clock;

internal sealed class UpdateProgramCommandValidator : AbstractValidator<UpdateProgramCommand>
{
    public UpdateProgramCommandValidator(ITimeZoneContext timeZoneContext)
    {
        RuleFor(x => x.Details).NotNull().SetValidator(new ProgramDetailsValidator(timeZoneContext));
    }
}
