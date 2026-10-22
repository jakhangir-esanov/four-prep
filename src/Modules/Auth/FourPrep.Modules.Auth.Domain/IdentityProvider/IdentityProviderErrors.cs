namespace FourPrep.Modules.Auth.Domain.IdentityProvider;

using FourPrep.Common.Domain;

public static class IdentityProviderErrors
{
    public static readonly Error UserNotFound = Error.NotFound(
        "IdentityProvider.UserNotFound",
        en: "The identity provider user was not found.",
        uz: "Foydalanuvchi identifikatsiya tizimida topilmadi.",
        ru: "Пользователь не найден в системе идентификации.");

    public static readonly Error Conflict = Error.Conflict(
        "IdentityProvider.Conflict",
        en: "The identity provider user operation conflicted with an existing identity.",
        uz: "Bunday foydalanuvchi allaqachon mavjud.",
        ru: "Такой пользователь уже существует.");

    public static readonly Error RoleNotFound = Error.NotFound(
        "IdentityProvider.RoleNotFound",
        en: "The realm role does not exist in the identity provider.",
        uz: "Bunday rol identifikatsiya tizimida mavjud emas.",
        ru: "Такая роль не существует в системе идентификации.");

    public static readonly Error OperationFailed = Error.Problem(
        "IdentityProvider.OperationFailed",
        en: "The identity provider user operation failed.",
        uz: "Identifikatsiya tizimidagi amal bajarilmadi.",
        ru: "Не удалось выполнить операцию в системе идентификации.");
}
