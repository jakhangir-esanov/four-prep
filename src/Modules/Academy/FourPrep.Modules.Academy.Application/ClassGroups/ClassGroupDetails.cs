namespace FourPrep.Modules.Academy.Application.ClassGroups;

using FourPrep.Common.Domain.Localization;
using FourPrep.Modules.Academy.Domain.ClassGroups;
using FourPrep.Modules.Academy.Domain.ScheduleSlots;

public sealed record ClassGroupDetails(
    string Code,
    ClassMode Mode,
    string? CountryCode,
    string? City,
    string? Address,
    IReadOnlyDictionary<string, string>? EntryLevel,
    string TimeZone,
    DateOnly StartsOn,
    int Weeks,
    int Capacity,
    IReadOnlyList<ScheduleSlotInput>? Schedule)
{
    public string NormalizedCode => Code.Trim().ToUpperInvariant();

    private bool IsOffice => Mode == ClassMode.Office;

    internal ClassGroup ToClassGroup(Guid courseId, DateTime utcNow) =>
        ClassGroup.Create(
            courseId,
            NormalizedCode,
            Mode,
            IsOffice ? Countries.NormalizeOrNull(CountryCode) : null,
            OfficeText(City),
            OfficeText(Address),
            LocalizedTexts.Normalize(EntryLevel),
            TimeZone.Trim(),
            StartsOn,
            Weeks,
            Capacity,
            Slots(),
            utcNow);

    internal void ApplyTo(ClassGroup classGroup, DateTime utcNow) =>
        classGroup.Update(
            NormalizedCode,
            Mode,
            IsOffice ? Countries.NormalizeOrNull(CountryCode) : null,
            OfficeText(City),
            OfficeText(Address),
            LocalizedTexts.Normalize(EntryLevel),
            TimeZone.Trim(),
            StartsOn,
            Weeks,
            Capacity,
            Slots(),
            utcNow);

    internal IReadOnlyList<ScheduleSlot> Slots() =>
        [.. (Schedule ?? []).Select(slot => slot.ToSlot()).OrderBy(slot => slot.Day).ThenBy(slot => slot.StartTime)];

    private string? OfficeText(string? value) =>
        IsOffice && !string.IsNullOrWhiteSpace(value) ? value.Trim() : null;
}
