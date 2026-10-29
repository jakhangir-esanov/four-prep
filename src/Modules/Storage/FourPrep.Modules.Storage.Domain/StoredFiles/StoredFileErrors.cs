namespace FourPrep.Modules.Storage.Domain.StoredFiles;

using FourPrep.Common.Domain;

public static class StoredFileErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "StoredFile.NotFound",
        en: "The specified file was not found.",
        uz: "Ko'rsatilgan fayl topilmadi.",
        ru: "Указанный файл не найден.");

    public static readonly Error UnsupportedType = Error.Problem(
        "StoredFile.UnsupportedType",
        en: "Only PDF, JPG, PNG and HEIC files can be uploaded, and the file name must end with a matching extension.",
        uz: "Faqat PDF, JPG, PNG va HEIC fayllarni yuklash mumkin, fayl nomi esa mos kengaytma bilan tugashi kerak.",
        ru: "Можно загружать только файлы PDF, JPG, PNG и HEIC, а имя файла должно заканчиваться соответствующим расширением.");

    public static readonly Error TooLarge = Error.Problem(
        "StoredFile.TooLarge",
        en: "A file can be at most 20 MB.",
        uz: "Fayl hajmi ko'pi bilan 20 MB bo'lishi mumkin.",
        ru: "Размер файла не может превышать 20 МБ.");

    public static readonly Error Empty = Error.Problem(
        "StoredFile.Empty",
        en: "The file is empty.",
        uz: "Fayl bo'sh.",
        ru: "Файл пуст.");

    public static readonly Error TooManyPending = Error.Conflict(
        "StoredFile.TooManyPending",
        en: "You have 10 uploads that are not finished yet. Finish or wait for them before starting more.",
        uz: "Sizda hali tugallanmagan 10 ta yuklash bor. Yangisini boshlashdan oldin ularni tugating yoki biroz kuting.",
        ru: "У вас 10 незавершённых загрузок. Завершите их или подождите, прежде чем начинать новые.");

    public static readonly Error NotUploaded = Error.Conflict(
        "StoredFile.NotUploaded",
        en: "The file has not reached storage yet. Upload it with the link first, then confirm again.",
        uz: "Fayl hali omborga yetib kelmagan. Avval uni havola orqali yuklang, keyin qayta tasdiqlang.",
        ru: "Файл ещё не попал в хранилище. Сначала загрузите его по ссылке, затем подтвердите снова.");

    public static readonly Error NotScanned = Error.Conflict(
        "StoredFile.NotScanned",
        en: "The file is still being checked for viruses. Try again in a few seconds.",
        uz: "Fayl hali virusga tekshirilmoqda. Bir necha soniyadan keyin qayta urinib ko'ring.",
        ru: "Файл ещё проверяется на вирусы. Попробуйте снова через несколько секунд.");

    public static readonly Error ScanFailed = Error.Conflict(
        "StoredFile.ScanFailed",
        en: "The file could not be checked for viruses. Upload it again.",
        uz: "Faylni virusga tekshirib bo'lmadi. Uni qaytadan yuklang.",
        ru: "Не удалось проверить файл на вирусы. Загрузите его заново.");

    public static readonly Error NotAwaitingScan = Error.Conflict(
        "StoredFile.NotAwaitingScan",
        en: "The file is not waiting for a virus check.",
        uz: "Fayl virus tekshiruvini kutmayapti.",
        ru: "Файл не ожидает проверки на вирусы.");

    public static readonly Error AlreadyUploaded = Error.Conflict(
        "StoredFile.AlreadyUploaded",
        en: "This upload has already been confirmed.",
        uz: "Bu yuklash allaqachon tasdiqlangan.",
        ru: "Эта загрузка уже подтверждена.");

    public static readonly Error SizeMismatch = Error.Problem(
        "StoredFile.SizeMismatch",
        en: "The uploaded file is not the size that was announced. Upload it again.",
        uz: "Yuklangan fayl hajmi oldin aytilganiga mos kelmaydi. Uni qaytadan yuklang.",
        ru: "Размер загруженного файла не совпадает с заявленным. Загрузите его заново.");

    public static readonly Error ContentMismatch = Error.Problem(
        "StoredFile.ContentMismatch",
        en: "The file contents do not match its type. Upload a real PDF, JPG, PNG or HEIC file.",
        uz: "Fayl mazmuni uning turiga mos kelmaydi. Haqiqiy PDF, JPG, PNG yoki HEIC fayl yuklang.",
        ru: "Содержимое файла не соответствует его типу. Загрузите настоящий файл PDF, JPG, PNG или HEIC.");
}
