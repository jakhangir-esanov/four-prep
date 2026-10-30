namespace FourPrep.Modules.Meeting.Application.StaffSchedules.SaveMySchedule;

using FluentValidation;

internal sealed class SaveMyScheduleCommandValidator : AbstractValidator<SaveMyScheduleCommand>
{
    public SaveMyScheduleCommandValidator()
    {
        RuleFor(x => x.Details).NotNull().SetValidator(new StaffScheduleDetailsValidator());
    }
}
