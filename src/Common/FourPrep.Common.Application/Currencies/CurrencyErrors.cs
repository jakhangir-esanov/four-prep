namespace FourPrep.Common.Application.Currencies;

using FourPrep.Common.Domain;

public static class CurrencyErrors
{
    public static readonly Error RateNotFound = Error.Problem(
        "Currency.RateNotFound",
        en: "There is no exchange rate for this currency.",
        uz: "Bu valyuta uchun kurs topilmadi.",
        ru: "Для этой валюты нет курса.");
}
