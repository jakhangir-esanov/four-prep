namespace FourPrep.Modules.Auth.Domain.UserSessions;

using FourPrep.Common.Domain;

public static class UserSessionErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "UserSession.NotFound",
        en: "The specified user session was not found.",
        uz: "Ko'rsatilgan foydalanuvchi sessiyasi topilmadi.",
        ru: "Указанная сессия пользователя не найдена.");
}
