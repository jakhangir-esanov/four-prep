namespace FourPrep.Modules.Admission.Domain.AcademyApplications;

using FourPrep.Common.Domain;

public static class AcademyApplicationErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "AcademyApplication.NotFound",
        en: "The specified application was not found.",
        uz: "Ko'rsatilgan ariza topilmadi.",
        ru: "Указанная заявка не найдена.");

    public static readonly Error NotYourStudent = Error.Forbidden(
        "AcademyApplication.NotYourStudent",
        en: "You can see applications only for yourself or for a child linked to your account.",
        uz: "Arizalarni faqat o'zingiz yoki hisobingizga bog'langan farzandingiz uchun ko'rishingiz mumkin.",
        ru: "Заявки можно видеть только свои или ребёнка, привязанного к вашему аккаунту.");

    public static readonly Error AlreadyApplied = Error.Conflict(
        "AcademyApplication.AlreadyApplied",
        en: "You already have an application that is under review or accepted, so a new one is not needed.",
        uz: "Sizda ko'rib chiqilayotgan yoki qabul qilingan ariza bor, shuning uchun yangisi kerak emas.",
        ru: "У вас уже есть заявка на рассмотрении или принятая заявка, поэтому новая не нужна.");

    public static readonly Error NotSubmitted = Error.Conflict(
        "AcademyApplication.NotSubmitted",
        en: "This application has already been reviewed or withdrawn.",
        uz: "Bu ariza allaqachon ko'rib chiqilgan yoki qaytarib olingan.",
        ru: "Эта заявка уже рассмотрена или отозвана.");

    public static readonly Error ParentConsentRequired = Error.Problem(
        "AcademyApplication.ParentConsentRequired",
        en: "Students under 18 must confirm that a parent knows about the application and agrees to it.",
        uz: "18 yoshga to'lmagan o'quvchi ota-onasi ariza haqida bilishi va rozi ekanini tasdiqlashi kerak.",
        ru: "Ученикам младше 18 лет нужно подтвердить, что родитель знает о заявке и согласен с ней.");

    public static readonly Error InterestRequired = Error.Problem(
        "AcademyApplication.InterestRequired",
        en: "Choose at least one thing you are interested in, each one once.",
        uz: "Sizni qiziqtirgan kamida bitta xizmatni tanlang, har birini bir martadan.",
        ru: "Выберите хотя бы одну услугу, которая вас интересует, каждую по одному разу.");

    public static readonly Error ContactChannelRequired = Error.Problem(
        "AcademyApplication.ContactChannelRequired",
        en: "Choose at least one way for us to contact you, each one once.",
        uz: "Siz bilan bog'lanishimiz uchun kamida bitta usulni tanlang, har birini bir martadan.",
        ru: "Выберите хотя бы один способ связи с вами, каждый по одному разу.");

    public static readonly Error PhoneRequired = Error.Problem(
        "AcademyApplication.PhoneRequired",
        en: "Give a phone number in international format, for example +14155550123, so we can call you.",
        uz: "Sizga qo'ng'iroq qilishimiz uchun telefon raqamini xalqaro formatda kiriting, masalan +14155550123.",
        ru: "Укажите номер телефона в международном формате, например +14155550123, чтобы мы могли вам позвонить.");

    public static readonly Error TelegramRequired = Error.Problem(
        "AcademyApplication.TelegramRequired",
        en: "Give a valid Telegram username: 5 to 32 letters, digits or underscores, starting with a letter.",
        uz: "To'g'ri Telegram username kiriting: harf bilan boshlanadigan 5 tadan 32 tagacha harf, raqam yoki pastki chiziq.",
        ru: "Укажите корректное имя пользователя в Telegram: от 5 до 32 букв, цифр или подчёркиваний, начиная с буквы.");

    public static readonly Error DeclineReasonRequired = Error.Problem(
        "AcademyApplication.DeclineReasonRequired",
        en: "Give the reason for declining, in at most 500 characters.",
        uz: "Rad etish sababini ko'pi bilan 500 ta belgida yozing.",
        ru: "Укажите причину отказа, не длиннее 500 символов.");
}
