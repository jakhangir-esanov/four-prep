namespace FourPrep.Modules.Academy.Domain.ScheduleSlots;

using FourPrep.Common.Domain;

public static class ScheduleSlotErrors
{
    public static readonly Error InvalidDuration = Error.Problem(
        "ScheduleSlot.InvalidDuration",
        en: "A lesson must last between 15 minutes and 4 hours and end on the same day.",
        uz: "Dars 15 daqiqadan 4 soatgacha davom etishi va o'sha kuni tugashi kerak.",
        ru: "Занятие должно длиться от 15 минут до 4 часов и заканчиваться в тот же день.");

    public static readonly Error Overlapping = Error.Problem(
        "ScheduleSlot.Overlapping",
        en: "Two lessons in the schedule overlap. Change the day or the time of one of them.",
        uz: "Jadvaldagi ikki dars vaqti ustma-ust tushyapti. Ulardan birining kuni yoki vaqtini o'zgartiring.",
        ru: "Два занятия в расписании пересекаются. Измените день или время одного из них.");
}
