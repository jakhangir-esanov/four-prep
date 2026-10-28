namespace FourPrep.Modules.Academy.Domain.Enrolments;

using FourPrep.Common.Domain;

public static class EnrolmentErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Enrolment.NotFound",
        en: "The specified enrolment was not found.",
        uz: "Ko'rsatilgan yozilish topilmadi.",
        ru: "Указанная запись на курс не найдена.");

    public static readonly Error NotYourStudent = Error.Forbidden(
        "Enrolment.NotYourStudent",
        en: "You can enrol only yourself, or a child linked to your account.",
        uz: "Faqat o'zingizni yoki hisobingizga bog'langan farzandingizni yozishingiz mumkin.",
        ru: "Записать можно только себя или ребёнка, привязанного к вашему аккаунту.");

    public static readonly Error AlreadyEnrolled = Error.Conflict(
        "Enrolment.AlreadyEnrolled",
        en: "The student is already enrolled in this group or on its waiting list.",
        uz: "O'quvchi bu guruhga allaqachon yozilgan yoki navbatda turibdi.",
        ru: "Ученик уже записан в эту группу или стоит в её листе ожидания.");

    public static readonly Error TooManyUnpaid = Error.Conflict(
        "Enrolment.TooManyUnpaid",
        en: "The student already has 3 unpaid enrolments or waiting-list places. Pay for one or cancel one first.",
        uz: "O'quvchida allaqachon 3 ta to'lanmagan yozilish yoki navbat bor. Avval birini to'lang yoki bekor qiling.",
        ru: "У ученика уже 3 неоплаченные записи или места в листе ожидания. Сначала оплатите или отмените одну из них.");

    public static readonly Error NotPendingPayment = Error.Conflict(
        "Enrolment.NotPendingPayment",
        en: "Only an enrolment that is waiting for payment can be activated.",
        uz: "Faqat to'lov kutilayotgan yozilishni faollashtirish mumkin.",
        ru: "Активировать можно только запись, ожидающую оплаты.");

    public static readonly Error CannotCancel = Error.Conflict(
        "Enrolment.CannotCancel",
        en: "A paid or finished enrolment cannot be cancelled here. Please contact the academy.",
        uz: "To'langan yoki tugagan yozilishni bu yerda bekor qilib bo'lmaydi. Akademiya bilan bog'laning.",
        ru: "Оплаченную или завершённую запись здесь отменить нельзя. Свяжитесь с академией.");

    public static readonly Error AlreadyEnded = Error.Conflict(
        "Enrolment.AlreadyEnded",
        en: "This enrolment has already ended.",
        uz: "Bu yozilish allaqachon tugagan.",
        ru: "Эта запись уже завершена.");
}
