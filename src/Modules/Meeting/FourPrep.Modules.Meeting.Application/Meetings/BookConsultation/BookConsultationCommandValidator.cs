namespace FourPrep.Modules.Meeting.Application.Meetings.BookConsultation;

using FluentValidation;
using FourPrep.Common.Application.Extensions;
using FourPrep.Modules.Meeting.Domain.Meetings;

internal sealed class BookConsultationCommandValidator : AbstractValidator<BookConsultationCommand>
{
    public BookConsultationCommandValidator()
    {
        RuleFor(x => x.StudentUserId).NotEmpty();
        RuleFor(x => x.StaffUserId).NotEmpty();
        RuleFor(x => x.StartsOnUtc).NotEmpty();
        RuleFor(x => x.Mode).IsInEnum();

        RuleFor(x => x.PhoneNumber).PhoneNumber();

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .When(x => x.Mode == MeetingMode.Phone)
            .WithError(MeetingErrors.PhoneRequired);
    }
}
