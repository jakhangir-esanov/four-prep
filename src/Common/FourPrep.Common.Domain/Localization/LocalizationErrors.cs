namespace FourPrep.Common.Domain.Localization;

public static class LocalizationErrors
{
    public static readonly Error InvalidCountry = Error.Problem(
        "Localization.InvalidCountry",
        en: "The country must be a two-letter ISO 3166-1 code, for example US, GB or UZ.",
        uz: "Davlat ikki harfli ISO 3166-1 kodi bo'lishi kerak, masalan US, GB yoki UZ.",
        ru: "Страна должна быть двухбуквенным кодом ISO 3166-1, например US, GB или UZ.");

    public static readonly Error InvalidTimeZone = Error.Problem(
        "Localization.InvalidTimeZone",
        en: "The time zone must be an IANA name, for example America/New_York or Asia/Tashkent.",
        uz: "Vaqt zonasi IANA nomi bo'lishi kerak, masalan America/New_York yoki Asia/Tashkent.",
        ru: "Часовой пояс должен быть названием IANA, например America/New_York или Asia/Tashkent.");

    public static readonly Error UnsupportedLocale = Error.Problem(
        "Localization.UnsupportedLocale",
        en: "The language must be one of: en, uz, ru.",
        uz: "Til quyidagilardan biri bo'lishi kerak: en, uz, ru.",
        ru: "Язык должен быть одним из: en, uz, ru.");

    public static readonly Error InvalidPhoneNumber = Error.Problem(
        "Localization.InvalidPhoneNumber",
        en: "The phone number must be in international format with the country code, for example +14155550100.",
        uz: "Telefon raqami davlat kodi bilan xalqaro formatda bo'lishi kerak, masalan +998901234567.",
        ru: "Номер телефона должен быть в международном формате с кодом страны, например +79161234567.");
}
