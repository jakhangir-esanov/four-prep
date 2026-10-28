namespace FourPrep.Modules.Catalogue.Domain.Facts;

using FourPrep.Common.Domain;

public static class FactErrors
{
    public static readonly Error InvalidSourceUrl = Error.Problem(
        "Fact.InvalidSourceUrl",
        en: "The source must be a full web address starting with http:// or https://.",
        uz: "Manba http:// yoki https:// bilan boshlanadigan to'liq veb-manzil bo'lishi kerak.",
        ru: "Источник должен быть полным веб-адресом, начинающимся с http:// или https://.");

    public static readonly Error CheckedOnInFuture = Error.Problem(
        "Fact.CheckedOnInFuture",
        en: "The date the data was checked cannot be in the future.",
        uz: "Ma'lumot tekshirilgan sana kelajakda bo'lishi mumkin emas.",
        ru: "Дата проверки данных не может быть в будущем.");
}
