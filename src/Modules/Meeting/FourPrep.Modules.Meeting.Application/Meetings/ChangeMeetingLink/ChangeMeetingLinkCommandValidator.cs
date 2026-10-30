namespace FourPrep.Modules.Meeting.Application.Meetings.ChangeMeetingLink;

using FluentValidation;
using FourPrep.Common.Application.Extensions;
using FourPrep.Modules.Meeting.Domain.StaffSchedules;

internal sealed class ChangeMeetingLinkCommandValidator : AbstractValidator<ChangeMeetingLinkCommand>
{
    public ChangeMeetingLinkCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.MeetingUrl)
            .NotEmpty()
            .MaximumLength(MeetingLinks.MaxLength)
            .Must(MeetingLinks.IsValid)
            .WithError(StaffScheduleErrors.InvalidMeetingUrl);
    }
}
