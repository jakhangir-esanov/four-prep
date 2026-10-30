namespace FourPrep.Modules.Meeting.Application.Meetings.GetConsultationSlots;

using FluentValidation;
using FourPrep.Common.Application.Extensions;
using FourPrep.Modules.Meeting.Domain.Meetings;

internal sealed class GetConsultationSlotsQueryValidator : AbstractValidator<GetConsultationSlotsQuery>
{
    private static readonly TimeSpan MaxRange = TimeSpan.FromDays(31);

    public GetConsultationSlotsQueryValidator()
    {
        RuleFor(x => x.StudentUserId).NotEmpty();
        RuleFor(x => x.Mode).IsInEnum();

        RuleFor(x => x.ToUtc)
            .Must((query, toUtc) => toUtc > query.FromUtc && toUtc - query.FromUtc <= MaxRange)
            .When(x => x.FromUtc is not null && x.ToUtc is not null)
            .WithError(MeetingErrors.InvalidRange);
    }
}
