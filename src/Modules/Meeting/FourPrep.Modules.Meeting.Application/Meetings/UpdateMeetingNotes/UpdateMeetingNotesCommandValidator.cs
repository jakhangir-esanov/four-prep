namespace FourPrep.Modules.Meeting.Application.Meetings.UpdateMeetingNotes;

using FluentValidation;

internal sealed class UpdateMeetingNotesCommandValidator : AbstractValidator<UpdateMeetingNotesCommand>
{
    private const int MaxNotesLength = 4000;

    public UpdateMeetingNotesCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Notes).MaximumLength(MaxNotesLength);
    }
}
