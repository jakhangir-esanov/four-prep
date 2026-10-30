namespace FourPrep.Modules.Meeting.Application.StaffSchedules;

using FluentValidation;
using FourPrep.Common.Application.Extensions;
using FourPrep.Modules.Meeting.Domain.Meetings;
using FourPrep.Modules.Meeting.Application.Meetings;
using FourPrep.Modules.Meeting.Domain.StaffSchedules;
using FourPrep.Modules.Meeting.Domain.AvailabilityWindows;

internal sealed class StaffScheduleDetailsValidator : AbstractValidator<StaffScheduleDetails>
{
    private const int MaxDisplayNameLength = 200;
    private const int MaxCityLength = 100;
    private const int MaxAddressLength = 300;
    private const int MaxWindows = 50;

    public StaffScheduleDetailsValidator()
    {
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(MaxDisplayNameLength);
        RuleFor(x => x.TimeZone).NotEmpty().TimeZoneId();

        RuleFor(x => x.MeetingUrl)
            .MaximumLength(MeetingLinks.MaxLength)
            .Must(url => string.IsNullOrWhiteSpace(url) || MeetingLinks.IsValid(url))
            .WithError(StaffScheduleErrors.InvalidMeetingUrl);

        RuleFor(x => x.MeetingUrl)
            .NotEmpty()
            .When(x => x.Offers(MeetingMode.Online))
            .WithError(StaffScheduleErrors.MeetingUrlRequired);

        RuleFor(x => x.OfficeCountryCode).CountryCode();
        RuleFor(x => x.OfficeCity).MaximumLength(MaxCityLength);
        RuleFor(x => x.OfficeAddress).MaximumLength(MaxAddressLength);

        When(x => x.Offers(MeetingMode.Office), () =>
        {
            RuleFor(x => x.OfficeCountryCode).NotEmpty().WithError(StaffScheduleErrors.OfficeRequired);
            RuleFor(x => x.OfficeCity).NotEmpty().WithError(StaffScheduleErrors.OfficeRequired);
            RuleFor(x => x.OfficeAddress).NotEmpty().WithError(StaffScheduleErrors.OfficeRequired);
        });

        RuleFor(x => x.Windows)
            .Must(windows => windows is null || windows.Count <= MaxWindows)
            .Must(HaveNoOverlaps)
            .WithError(AvailabilityWindowErrors.Overlapping);

        RuleForEach(x => x.Windows).NotNull().ChildRules(window =>
        {
            window.RuleFor(x => x.Day).IsInEnum();
            window.RuleFor(x => x.Mode).IsInEnum();
            window.RuleFor(x => x.EndTime)
                .Must((input, end) => end.ToTimeSpan() - input.StartTime.ToTimeSpan() >= ConsultationSlots.Duration)
                .WithError(AvailabilityWindowErrors.TooShort);
        });
    }

    private static bool HaveNoOverlaps(IReadOnlyList<AvailabilityWindowInput>? windows)
    {
        var given = (windows ?? []).OfType<AvailabilityWindowInput>().Select(input => input.ToWindow()).ToList();

        return !given.Where((window, index) => given.Skip(index + 1).Any(window.Overlaps)).Any();
    }
}
