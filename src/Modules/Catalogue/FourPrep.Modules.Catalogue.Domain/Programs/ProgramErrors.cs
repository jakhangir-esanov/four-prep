namespace FourPrep.Modules.Catalogue.Domain.Programs;

using FourPrep.Common.Domain;

public static class ProgramErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Program.NotFound",
        en: "The specified program was not found.",
        uz: "Ko'rsatilgan dastur topilmadi.",
        ru: "Указанная программа не найдена.");

    public static readonly Error NameTaken = Error.Conflict(
        "Program.NameTaken",
        en: "This university already has a program with this name.",
        uz: "Bu universitetda shu nomli dastur allaqachon bor.",
        ru: "У этого университета уже есть программа с таким названием.");

    public static readonly Error InvalidLanguage = Error.Problem(
        "Program.InvalidLanguage",
        en: "The language of instruction must be a two-letter ISO 639-1 code, for example en, de or fr.",
        uz: "O'qitish tili ikki harfli ISO 639-1 kodi bo'lishi kerak, masalan en, de yoki fr.",
        ru: "Язык обучения должен быть двухбуквенным кодом ISO 639-1, например en, de или fr.");

    public static readonly Error InvalidDuration = Error.Problem(
        "Program.InvalidDuration",
        en: "The duration must be between 1 and 120 months.",
        uz: "Davomiylik 1 oydan 120 oygacha bo'lishi kerak.",
        ru: "Продолжительность должна быть от 1 до 120 месяцев.");
}
