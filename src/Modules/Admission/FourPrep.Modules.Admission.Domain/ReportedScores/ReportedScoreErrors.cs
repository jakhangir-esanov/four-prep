namespace FourPrep.Modules.Admission.Domain.ReportedScores;

using FourPrep.Common.Domain;

public static class ReportedScoreErrors
{
    public static readonly Error DuplicateExam = Error.Problem(
        "ReportedScore.DuplicateExam",
        en: "Give at most one score per exam.",
        uz: "Har bir imtihon uchun ko'pi bilan bitta ball kiriting.",
        ru: "Укажите не больше одного балла на каждый экзамен.");

    public static readonly Error TooManyScores = Error.Problem(
        "ReportedScore.TooManyScores",
        en: "Give at most 4 scores.",
        uz: "Ko'pi bilan 4 ta ball kiriting.",
        ru: "Укажите не больше 4 баллов.");

    public static readonly Error InvalidScore = Error.Problem(
        "ReportedScore.InvalidScore",
        en: "This score is not possible for this exam. Check the exam and the score.",
        uz: "Bu imtihonda bunday ball bo'lmaydi. Imtihon va ballni tekshiring.",
        ru: "Такого балла у этого экзамена не бывает. Проверьте экзамен и балл.");
}
