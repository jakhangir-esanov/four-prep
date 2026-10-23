namespace FourPrep.Modules.Student.Domain.ExamGoals;

using FourPrep.Common.Domain;

public static class ExamGoalErrors
{
    public static readonly Error InvalidTargetScore = Error.Problem(
        "ExamGoal.InvalidTargetScore",
        en: "The target score is not valid for this exam.",
        uz: "Maqsad ball bu imtihon uchun noto'g'ri.",
        ru: "Целевой балл недопустим для этого экзамена.");

    public static readonly Error DuplicateExam = Error.Problem(
        "ExamGoal.DuplicateExam",
        en: "Each exam may appear only once.",
        uz: "Har bir imtihon faqat bir marta ko'rsatilishi mumkin.",
        ru: "Каждый экзамен можно указать только один раз.");
}
