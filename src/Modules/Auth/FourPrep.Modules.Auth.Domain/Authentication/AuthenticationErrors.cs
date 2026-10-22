namespace FourPrep.Modules.Auth.Domain.Authentication;

using FourPrep.Common.Domain;

public static class AuthenticationErrors
{
    public static readonly Error InvalidCredentials = Error.Problem(
        "Authentication.InvalidCredentials",
        en: "The provided credentials are invalid.",
        uz: "Kiritilgan login yoki parol noto'g'ri.",
        ru: "Неверный логин или пароль.");

    public static readonly Error InvalidRefreshToken = Error.Problem(
        "Authentication.InvalidRefreshToken",
        en: "The provided refresh token is invalid.",
        uz: "Sessiya muddati tugagan yoki yaroqsiz. Qaytadan kiring.",
        ru: "Сессия истекла или недействительна. Войдите снова.");

    public static readonly Error LogoutFailed = Error.Problem(
        "Authentication.LogoutFailed",
        en: "The logout operation failed.",
        uz: "Tizimdan chiqishda xatolik yuz berdi.",
        ru: "Не удалось выйти из системы.");
}
