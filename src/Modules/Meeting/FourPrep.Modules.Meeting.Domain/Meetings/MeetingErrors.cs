namespace FourPrep.Modules.Meeting.Domain.Meetings;

using FourPrep.Common.Domain;

public static class MeetingErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Meeting.NotFound",
        en: "The specified meeting was not found.",
        uz: "Ko'rsatilgan uchrashuv topilmadi.",
        ru: "Указанная встреча не найдена.");

    public static readonly Error NotYourStudent = Error.Forbidden(
        "Meeting.NotYourStudent",
        en: "You can book and see meetings only for yourself or for a child linked to your account.",
        uz: "Uchrashuvni faqat o'zingiz yoki hisobingizga bog'langan farzandingiz uchun bron qilishingiz va ko'rishingiz mumkin.",
        ru: "Бронировать и видеть встречи можно только для себя или для ребёнка, привязанного к вашему аккаунту.");

    public static readonly Error SlotNotAvailable = Error.Conflict(
        "Meeting.SlotNotAvailable",
        en: "This time is not offered for booking. Choose one of the free times.",
        uz: "Bu vaqtni bron qilib bo'lmaydi. Bo'sh vaqtlardan birini tanlang.",
        ru: "Это время недоступно для записи. Выберите одно из свободных.");

    public static readonly Error SlotTaken = Error.Conflict(
        "Meeting.SlotTaken",
        en: "Someone has just booked this time. Choose another free time.",
        uz: "Bu vaqtni hozirgina boshqa kishi bron qildi. Boshqa bo'sh vaqtni tanlang.",
        ru: "Это время только что заняли. Выберите другое свободное время.");

    public static readonly Error AlreadyBooked = Error.Conflict(
        "Meeting.AlreadyBooked",
        en: "This student already has a consultation booked. Cancel it before booking another one.",
        uz: "Bu o'quvchida allaqachon bron qilingan konsultatsiya bor. Boshqasini bron qilishdan oldin uni bekor qiling.",
        ru: "У этого ученика уже есть запись на консультацию. Отмените её, прежде чем записываться снова.");

    public static readonly Error TooManyBooked = Error.Conflict(
        "Meeting.TooManyBooked",
        en: "You already have 3 upcoming consultations booked. Wait for one of them or cancel it before booking another.",
        uz: "Sizda allaqachon 3 ta kelgusi konsultatsiya bron qilingan. Yangisini bron qilishdan oldin ulardan biri o'tishini kuting yoki uni bekor qiling.",
        ru: "У вас уже есть 3 предстоящие консультации. Дождитесь одной из них или отмените её, прежде чем записываться снова.");

    public static readonly Error MentorOnly = Error.Conflict(
        "Meeting.MentorOnly",
        en: "This student has a mentor, so the consultation is booked with the mentor. Choose one of the mentor's free times.",
        uz: "Bu o'quvchining mentori bor, shuning uchun konsultatsiya mentor bilan bron qilinadi. Mentorning bo'sh vaqtlaridan birini tanlang.",
        ru: "У этого ученика есть ментор, поэтому консультация записывается к нему. Выберите одно из свободных времён ментора.");

    public static readonly Error PhoneRequired = Error.Problem(
        "Meeting.PhoneRequired",
        en: "Give a phone number in international format, for example +14155550123, so we can call you.",
        uz: "Sizga qo'ng'iroq qilishimiz uchun telefon raqamini xalqaro formatda kiriting, masalan +14155550123.",
        ru: "Укажите номер телефона в международном формате, например +14155550123, чтобы мы могли вам позвонить.");

    public static readonly Error InvalidRange = Error.Problem(
        "Meeting.InvalidRange",
        en: "The end of the period must come after its start, and the period can be at most 31 days.",
        uz: "Davr oxiri boshidan keyin bo'lishi kerak, davr esa ko'pi bilan 31 kun.",
        ru: "Конец периода должен быть позже начала, а сам период — не длиннее 31 дня.");

    public static readonly Error NotScheduled = Error.Conflict(
        "Meeting.NotScheduled",
        en: "This meeting is no longer scheduled.",
        uz: "Bu uchrashuv endi rejada emas.",
        ru: "Эта встреча больше не запланирована.");

    public static readonly Error AlreadyStarted = Error.Conflict(
        "Meeting.AlreadyStarted",
        en: "The meeting has already started, so it can no longer be cancelled here.",
        uz: "Uchrashuv allaqachon boshlangan, uni endi bu yerda bekor qilib bo'lmaydi.",
        ru: "Встреча уже началась, поэтому здесь её больше нельзя отменить.");

    public static readonly Error NotStartedYet = Error.Conflict(
        "Meeting.NotStartedYet",
        en: "Attendance can be recorded only after the meeting has started.",
        uz: "Qatnashuvni faqat uchrashuv boshlangandan keyin belgilash mumkin.",
        ru: "Отметить посещение можно только после начала встречи.");

    public static readonly Error InvalidAttendance = Error.Problem(
        "Meeting.InvalidAttendance",
        en: "Attendance is either held (3) or no-show (4).",
        uz: "Qatnashuv yo o'tkazildi (3), yo kelmadi (4) bo'ladi.",
        ru: "Посещение отмечается как состоялась (3) или неявка (4).");

    public static readonly Error NotOnline = Error.Conflict(
        "Meeting.NotOnline",
        en: "Only an online meeting has a link.",
        uz: "Havola faqat online uchrashuvda bo'ladi.",
        ru: "Ссылка есть только у онлайн-встречи.");
}
