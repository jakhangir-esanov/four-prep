namespace FourPrep.Modules.Meeting.Domain.StaffSchedules;

using FourPrep.Common.Domain;

public static class StaffScheduleErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "StaffSchedule.NotFound",
        en: "You have not set up your meeting schedule yet.",
        uz: "Siz hali uchrashuvlar jadvalingizni sozlamagansiz.",
        ru: "Вы ещё не настроили расписание встреч.");

    public static readonly Error MeetingUrlRequired = Error.Problem(
        "StaffSchedule.MeetingUrlRequired",
        en: "Add your Zoom or Google Meet link to offer online consultations.",
        uz: "Online konsultatsiya o'tkazish uchun Zoom yoki Google Meet havolangizni kiriting.",
        ru: "Чтобы проводить онлайн-консультации, добавьте ссылку на Zoom или Google Meet.");

    public static readonly Error InvalidMeetingUrl = Error.Problem(
        "StaffSchedule.InvalidMeetingUrl",
        en: "The meeting link must be a full https:// Zoom or Google Meet address, for example https://zoom.us/j/1234567890 or https://meet.google.com/abc-defg-hij.",
        uz: "Uchrashuv havolasi to'liq https:// Zoom yoki Google Meet manzili bo'lishi kerak, masalan https://zoom.us/j/1234567890 yoki https://meet.google.com/abc-defg-hij.",
        ru: "Ссылка на встречу должна быть полным адресом https:// Zoom или Google Meet, например https://zoom.us/j/1234567890 или https://meet.google.com/abc-defg-hij.");

    public static readonly Error OfficeRequired = Error.Problem(
        "StaffSchedule.OfficeRequired",
        en: "Add the office country, city and address to offer consultations in the office.",
        uz: "Ofisda konsultatsiya o'tkazish uchun ofis joylashgan davlat, shahar va manzilni kiriting.",
        ru: "Чтобы проводить консультации в офисе, укажите страну, город и адрес офиса.");
}
