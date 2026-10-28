namespace FourPrep.Modules.Catalogue.Domain.Costs;

using FourPrep.Common.Domain;

public static class CostErrors
{
    public static readonly Error DuplicateAcademicYear = Error.Problem(
        "Cost.DuplicateAcademicYear",
        en: "Each academic year may have only one cost entry.",
        uz: "Har bir o'quv yili uchun faqat bitta xarajat yozuvi bo'lishi mumkin.",
        ru: "Для каждого учебного года допускается только одна запись о стоимости.");

    public static readonly Error InvalidAcademicYear = Error.Problem(
        "Cost.InvalidAcademicYear",
        en: "The academic year is the year it starts, for example 2026 for 2026/27.",
        uz: "O'quv yili u boshlanadigan yil bilan ko'rsatiladi, masalan 2026/27 uchun 2026.",
        ru: "Учебный год указывается годом начала, например 2026 для 2026/27.");

    public static readonly Error InvalidAmount = Error.Problem(
        "Cost.InvalidAmount",
        en: "Amounts are whole numbers in the currency's smallest unit and cannot be negative.",
        uz: "Summalar valyutaning eng kichik birligida butun son bilan yoziladi va manfiy bo'lmaydi.",
        ru: "Суммы указываются целым числом в минимальных единицах валюты и не могут быть отрицательными.");
}
