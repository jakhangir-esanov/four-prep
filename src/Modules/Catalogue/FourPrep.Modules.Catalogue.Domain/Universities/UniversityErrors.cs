namespace FourPrep.Modules.Catalogue.Domain.Universities;

using FourPrep.Common.Domain;

public static class UniversityErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "University.NotFound",
        en: "The specified university was not found.",
        uz: "Ko'rsatilgan universitet topilmadi.",
        ru: "Указанный университет не найден.");

    public static readonly Error SlugTaken = Error.Conflict(
        "University.SlugTaken",
        en: "Another university already uses this address name. Choose a different one.",
        uz: "Bu manzil nomi boshqa universitetga berilgan. Boshqasini tanlang.",
        ru: "Это имя адреса уже занято другим университетом. Выберите другое.");

    public static readonly Error InvalidSlug = Error.Problem(
        "University.InvalidSlug",
        en: "The address name may contain only lowercase Latin letters, digits and single hyphens, for example mit or tu-munich.",
        uz: "Manzil nomida faqat kichik lotin harflari, raqamlar va yakka chiziqcha bo'lishi mumkin, masalan mit yoki tu-munich.",
        ru: "Имя адреса может содержать только строчные латинские буквы, цифры и одиночные дефисы, например mit или tu-munich.");

    public static readonly Error InvalidWebsite = Error.Problem(
        "University.InvalidWebsite",
        en: "The website must be a full web address starting with http:// or https://.",
        uz: "Veb-sayt http:// yoki https:// bilan boshlanadigan to'liq manzil bo'lishi kerak.",
        ru: "Сайт должен быть полным адресом, начинающимся с http:// или https://.");

    public static readonly Error InvalidAcceptanceRate = Error.Problem(
        "University.InvalidAcceptanceRate",
        en: "The acceptance rate must be a percentage between 0 and 100.",
        uz: "Qabul foizi 0 dan 100 gacha bo'lishi kerak.",
        ru: "Процент зачисления должен быть от 0 до 100.");

    public static readonly Error RankingSourceRequired = Error.Problem(
        "University.RankingSourceRequired",
        en: "A ranking needs its source, for example QS World University Rankings 2026.",
        uz: "Reyting o'rni manbasi bilan birga kiritiladi, masalan QS World University Rankings 2026.",
        ru: "Для места в рейтинге нужно указать источник, например QS World University Rankings 2026.");
}
