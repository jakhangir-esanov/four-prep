namespace FourPrep.Modules.Catalogue.Domain.Imports;

using FourPrep.Common.Domain;

public static class ImportErrors
{
    public static readonly Error UnreadableFile = Error.Problem(
        "Import.UnreadableFile",
        en: "The file could not be read as CSV. Save it as CSV (UTF-8) and try again.",
        uz: "Faylni CSV sifatida o'qib bo'lmadi. Uni CSV (UTF-8) formatida saqlab, qaytadan yuklang.",
        ru: "Не удалось прочитать файл как CSV. Сохраните его в формате CSV (UTF-8) и попробуйте снова.");

    public static readonly Error EmptyFile = Error.Problem(
        "Import.EmptyFile",
        en: "The file has no rows below the header.",
        uz: "Faylda sarlavhadan keyin birorta ham qator yo'q.",
        ru: "В файле нет строк под заголовком.");

    public static readonly Error TooManyRows = Error.Problem(
        "Import.TooManyRows",
        en: "The file has too many rows. Split it into files of at most 5,000 rows.",
        uz: "Faylda qatorlar juda ko'p. Uni ko'pi bilan 5 000 qatorli fayllarga bo'ling.",
        ru: "В файле слишком много строк. Разбейте его на файлы не более чем по 5 000 строк.");

    public static readonly Error MissingColumn = Error.Problem(
        "Import.MissingColumn",
        en: "A required column is missing from the header.",
        uz: "Sarlavhada majburiy ustun yo'q.",
        ru: "В заголовке нет обязательного столбца.");

    public static readonly Error UnknownColumn = Error.Problem(
        "Import.UnknownColumn",
        en: "The header has a column this import does not know. Check its spelling.",
        uz: "Sarlavhada bu import bilmaydigan ustun bor. Uning yozilishini tekshiring.",
        ru: "В заголовке есть столбец, который этот импорт не знает. Проверьте его написание.");

    public static readonly Error InvalidValue = Error.Problem(
        "Import.InvalidValue",
        en: "The value cannot be read. Numbers use a dot, dates are YYYY-MM-DD and yes/no is true or false.",
        uz: "Qiymatni o'qib bo'lmadi. Sonlarda nuqta ishlatiladi, sana YYYY-MM-DD ko'rinishida, ha/yo'q esa true yoki false.",
        ru: "Не удалось прочитать значение. В числах используется точка, даты в формате YYYY-MM-DD, да/нет — true или false.");

    public static readonly Error DuplicateRow = Error.Problem(
        "Import.DuplicateRow",
        en: "The same record appears more than once in the file.",
        uz: "Bitta yozuv faylda bir necha marta uchraydi.",
        ru: "Одна и та же запись встречается в файле несколько раз.");

    public static readonly Error UnknownUniversity = Error.Problem(
        "Import.UnknownUniversity",
        en: "No university has this address name. Import or create the university first.",
        uz: "Bu manzil nomli universitet yo'q. Avval universitetni import qiling yoki yarating.",
        ru: "Университета с таким именем адреса нет. Сначала импортируйте или создайте университет.");

    public static readonly Error UnknownProgram = Error.Problem(
        "Import.UnknownProgram",
        en: "This university has no program with this exact name. Import or create the program first.",
        uz: "Bu universitetda aynan shu nomli dastur yo'q. Avval dasturni import qiling yoki yarating.",
        ru: "У этого университета нет программы с точно таким названием. Сначала импортируйте или создайте программу.");
}
