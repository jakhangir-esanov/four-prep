namespace FourPrep.Modules.Student.Domain.Students;

using FourPrep.Common.Domain;

public static class StudentErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Student.NotFound",
        en: "The specified student was not found.",
        uz: "Ko'rsatilgan o'quvchi topilmadi.",
        ru: "Указанный ученик не найден.");

    public static readonly Error LinkCodeNotFound = Error.NotFound(
        "Student.LinkCodeNotFound",
        en: "No student has this link code. Check the code and try again.",
        uz: "Bu kod bilan o'quvchi topilmadi. Kodni tekshirib, qaytadan kiriting.",
        ru: "Ученик с таким кодом не найден. Проверьте код и попробуйте снова.");

    public static readonly Error LinkCodeUnavailable = Error.Problem(
        "Student.LinkCodeUnavailable",
        en: "A unique link code could not be generated. Please try again.",
        uz: "Yangi bog'lash kodini yaratib bo'lmadi. Qaytadan urinib ko'ring.",
        ru: "Не удалось создать уникальный код. Попробуйте ещё раз.");

    public static readonly Error NotAMentor = Error.Problem(
        "Student.NotAMentor",
        en: "The specified user is not a mentor.",
        uz: "Ko'rsatilgan foydalanuvchi mentor emas.",
        ru: "Указанный пользователь не является ментором.");

    public static readonly Error NoMentor = Error.Conflict(
        "Student.NoMentor",
        en: "The student has no mentor assigned.",
        uz: "O'quvchiga mentor biriktirilmagan.",
        ru: "Ученику не назначен ментор.");

    public static readonly Error InvalidGrade = Error.Problem(
        "Student.InvalidGrade",
        en: "Grade must be between 1 and 12.",
        uz: "Sinf 1 dan 12 gacha bo'lishi kerak.",
        ru: "Класс должен быть от 1 до 12.");

    public static readonly Error InvalidDateOfBirth = Error.Problem(
        "Student.InvalidDateOfBirth",
        en: "The date of birth is not valid.",
        uz: "Tug'ilgan sana noto'g'ri.",
        ru: "Неверная дата рождения.");
}
