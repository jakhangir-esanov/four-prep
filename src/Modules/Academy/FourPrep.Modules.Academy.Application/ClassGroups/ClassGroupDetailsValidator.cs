namespace FourPrep.Modules.Academy.Application.ClassGroups;

using FluentValidation;
using System.Text.RegularExpressions;
using FourPrep.Common.Application.Extensions;
using FourPrep.Modules.Academy.Domain.ClassGroups;
using FourPrep.Modules.Academy.Domain.ScheduleSlots;

internal sealed partial class ClassGroupDetailsValidator : AbstractValidator<ClassGroupDetails>
{
    private const int MaxCodeLength = 50;
    private const int MaxCityLength = 100;
    private const int MaxAddressLength = 300;
    private const int MaxEntryLevelLength = 500;
    private const int MaxWeeks = 104;
    private const int MaxCapacity = 200;
    private const int MaxLessonsPerWeek = 14;
    private const int MinLessonMinutes = 15;
    private const int MaxLessonMinutes = 240;

    private static readonly TimeSpan OneDay = TimeSpan.FromDays(1);

    public ClassGroupDetailsValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(MaxCodeLength)
            .Must(code => string.IsNullOrWhiteSpace(code) || CodePattern().IsMatch(code.Trim().ToUpperInvariant()))
            .WithError(ClassGroupErrors.InvalidCode);

        RuleFor(x => x.Mode).IsInEnum();

        RuleFor(x => x.CountryCode).CountryCode();
        RuleFor(x => x.City).MaximumLength(MaxCityLength);
        RuleFor(x => x.Address).MaximumLength(MaxAddressLength);

        When(x => x.Mode == ClassMode.Office, () =>
        {
            RuleFor(x => x.CountryCode).NotEmpty().WithError(ClassGroupErrors.OfficeLocationRequired);
            RuleFor(x => x.City).NotEmpty().WithError(ClassGroupErrors.OfficeLocationRequired);
            RuleFor(x => x.Address).NotEmpty().WithError(ClassGroupErrors.OfficeLocationRequired);
        });

        RuleFor(x => x.EntryLevel).LocalizedText(MaxEntryLevelLength);
        RuleFor(x => x.TimeZone).NotEmpty().TimeZoneId();
        RuleFor(x => x.StartsOn).NotEmpty();
        RuleFor(x => x.Weeks).InclusiveBetween(1, MaxWeeks);
        RuleFor(x => x.Capacity).InclusiveBetween(1, MaxCapacity);

        RuleFor(x => x.Schedule)
            .NotEmpty()
            .WithError(ClassGroupErrors.ScheduleRequired)
            .Must(schedule => schedule is null || schedule.Count <= MaxLessonsPerWeek)
            .Must(HaveNoOverlaps)
            .WithError(ScheduleSlotErrors.Overlapping);

        RuleForEach(x => x.Schedule).NotNull().ChildRules(slot =>
        {
            slot.RuleFor(x => x.Day).IsInEnum();
            slot.RuleFor(x => x.DurationMinutes)
                .InclusiveBetween(MinLessonMinutes, MaxLessonMinutes)
                .WithError(ScheduleSlotErrors.InvalidDuration)
                .Must((input, minutes) => input.StartTime.ToTimeSpan() + TimeSpan.FromMinutes(minutes) <= OneDay)
                .WithError(ScheduleSlotErrors.InvalidDuration);
        });
    }

    private static bool HaveNoOverlaps(IReadOnlyList<ScheduleSlotInput>? schedule)
    {
        var slots = (schedule ?? []).OfType<ScheduleSlotInput>().Select(input => input.ToSlot()).ToList();

        return !slots.Where((slot, index) => slots.Skip(index + 1).Any(slot.Overlaps)).Any();
    }

    [GeneratedRegex("^[A-Z0-9]+(-[A-Z0-9]+)*$")]
    private static partial Regex CodePattern();
}
