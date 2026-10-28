namespace FourPrep.Modules.Catalogue.Domain.Deadlines;

using FourPrep.Common.Domain;

public static class DeadlineErrors
{
    public static readonly Error DuplicateRound = Error.Problem(
        "Deadline.DuplicateRound",
        en: "Each application round may appear only once.",
        uz: "Har bir ariza raundi faqat bir marta ko'rsatilishi mumkin.",
        ru: "Каждый этап подачи можно указать только один раз.");
}
