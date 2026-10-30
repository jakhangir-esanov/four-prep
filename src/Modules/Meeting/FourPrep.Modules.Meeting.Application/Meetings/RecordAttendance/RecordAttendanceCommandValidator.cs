namespace FourPrep.Modules.Meeting.Application.Meetings.RecordAttendance;

using FluentValidation;
using FourPrep.Common.Application.Extensions;
using FourPrep.Modules.Meeting.Domain.Meetings;

internal sealed class RecordAttendanceCommandValidator : AbstractValidator<RecordAttendanceCommand>
{
    public RecordAttendanceCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Status)
            .Must(status => status is MeetingStatus.Held or MeetingStatus.NoShow)
            .WithError(MeetingErrors.InvalidAttendance);
    }
}
