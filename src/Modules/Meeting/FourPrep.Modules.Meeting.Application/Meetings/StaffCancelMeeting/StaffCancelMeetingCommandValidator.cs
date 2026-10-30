namespace FourPrep.Modules.Meeting.Application.Meetings.StaffCancelMeeting;

using FluentValidation;

internal sealed class StaffCancelMeetingCommandValidator : AbstractValidator<StaffCancelMeetingCommand>
{
    private const int MaxReasonLength = 500;

    public StaffCancelMeetingCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Reason).MaximumLength(MaxReasonLength);
    }
}
