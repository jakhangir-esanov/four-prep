namespace FourPrep.Modules.Meeting.Domain.AvailabilityWindows;

using FourPrep.Common.Domain;

public static class AvailabilityWindowErrors
{
    public static readonly Error TooShort = Error.Problem(
        "AvailabilityWindow.TooShort",
        en: "An available time must end after it starts and be long enough for one 45-minute consultation.",
        uz: "Bo'sh vaqt boshlanishidan keyin tugashi va kamida bitta 45 daqiqalik konsultatsiyaga yetishi kerak.",
        ru: "Свободное время должно заканчиваться после начала и вмещать хотя бы одну 45-минутную консультацию.");

    public static readonly Error Overlapping = Error.Problem(
        "AvailabilityWindow.Overlapping",
        en: "Two available times of the same format overlap on one day. Join them or change one of them.",
        uz: "Bir kunda bir xil formatdagi ikki bo'sh vaqt ustma-ust tushyapti. Ularni birlashtiring yoki birini o'zgartiring.",
        ru: "Два свободных интервала одного формата пересекаются в один день. Объедините их или измените один из них.");
}
