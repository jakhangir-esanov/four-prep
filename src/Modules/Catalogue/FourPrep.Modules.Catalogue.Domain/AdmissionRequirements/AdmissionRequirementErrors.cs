namespace FourPrep.Modules.Catalogue.Domain.AdmissionRequirements;

using FourPrep.Common.Domain;

public static class AdmissionRequirementErrors
{
    public static readonly Error DuplicateIntake = Error.Problem(
        "AdmissionRequirement.DuplicateIntake",
        en: "Each intake may have only one set of requirements.",
        uz: "Har bir qabul mavsumi uchun faqat bitta talablar to'plami bo'lishi mumkin.",
        ru: "Для каждого набора допускается только один список требований.");

    public static readonly Error InvalidIntakeYear = Error.Problem(
        "AdmissionRequirement.InvalidIntakeYear",
        en: "The intake year is not valid.",
        uz: "Qabul yili noto'g'ri.",
        ru: "Год набора указан неверно.");

    public static readonly Error InvalidEnglishMinimum = Error.Problem(
        "AdmissionRequirement.InvalidEnglishMinimum",
        en: "English minimums accept IELTS, TOEFL and Duolingo scores that exist on each test's scale.",
        uz: "Ingliz tili minimumi uchun IELTS, TOEFL va Duolingo ballari har bir testning shkalasida bo'lishi kerak.",
        ru: "Минимальный уровень английского принимает баллы IELTS, TOEFL и Duolingo, допустимые по шкале каждого теста.");

    public static readonly Error InvalidSatRange = Error.Problem(
        "AdmissionRequirement.InvalidSatRange",
        en: "The SAT middle 50% range needs two valid SAT scores, the lower one first.",
        uz: "SAT o'rtacha 50% oralig'i ikkita to'g'ri SAT ballidan iborat, avval kichigi yoziladi.",
        ru: "Средний диапазон SAT (50%) состоит из двух допустимых баллов SAT, сначала меньший.");

    public static readonly Error InvalidGpa = Error.Problem(
        "AdmissionRequirement.InvalidGpa",
        en: "The minimum GPA needs its scale, for example 3.5 on a 4.0 scale, and cannot exceed it.",
        uz: "Minimal GPA shkalasi bilan kiritiladi, masalan 4.0 shkalada 3.5, va shkaladan oshmaydi.",
        ru: "Минимальный GPA указывается вместе со шкалой, например 3.5 по шкале 4.0, и не может её превышать.");
}
