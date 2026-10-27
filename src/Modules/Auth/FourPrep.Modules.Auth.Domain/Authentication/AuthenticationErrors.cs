namespace FourPrep.Modules.Auth.Domain.Authentication;

using FourPrep.Common.Domain;

public static class AuthenticationErrors
{
    public static readonly Error InvalidCredentials = Error.Problem(
        "Authentication.InvalidCredentials",
        en: "The provided credentials are invalid.",
        uz: "Kiritilgan login yoki parol noto'g'ri.",
        ru: "Неверный логин или пароль.");

    public static readonly Error InvalidGoogleToken = Error.Problem(
        "Authentication.InvalidGoogleToken",
        en: "Signing in with Google failed. Please try again.",
        uz: "Google orqali kirib bo'lmadi. Qaytadan urinib ko'ring.",
        ru: "Не удалось войти через Google. Попробуйте ещё раз.");

    public static readonly Error GoogleSignInNotAllowed = Error.Forbidden(
        "Authentication.GoogleSignInNotAllowed",
        en: "Staff accounts cannot sign in with Google. Sign in to the staff panel with your username and password.",
        uz: "Xodim hisoblari Google orqali kira olmaydi. Xodimlar paneliga login va parolingiz bilan kiring.",
        ru: "Сотрудники не могут входить через Google. Войдите в панель сотрудников по логину и паролю.");

    public static readonly Error SignUpDetailsRequired = Error.Problem(
        "Authentication.SignUpDetailsRequired",
        en: "This Google account is new to 4Prep. Choose student or parent and your country to finish signing up.",
        uz: "Bu Google hisobi 4Prep'da yangi. Ro'yxatdan o'tishni tugatish uchun o'quvchi yoki ota-ona ekaningizni va davlatingizni tanlang.",
        ru: "Этот аккаунт Google новый для 4Prep. Чтобы завершить регистрацию, выберите, ученик вы или родитель, и укажите страну.");

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
