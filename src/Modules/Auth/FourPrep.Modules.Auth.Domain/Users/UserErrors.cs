namespace FourPrep.Modules.Auth.Domain.Users;

using FourPrep.Common.Domain;

public static class UserErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "User.NotFound",
        en: "The specified user was not found.",
        uz: "Ko'rsatilgan foydalanuvchi topilmadi.",
        ru: "Указанный пользователь не найден.");

    public static readonly Error AlreadyBlocked = Error.Conflict(
        "User.AlreadyBlocked",
        en: "The user is already blocked.",
        uz: "Foydalanuvchi allaqachon bloklangan.",
        ru: "Пользователь уже заблокирован.");

    public static readonly Error NotBlocked = Error.Conflict(
        "User.NotBlocked",
        en: "The user is not blocked.",
        uz: "Foydalanuvchi bloklanmagan.",
        ru: "Пользователь не заблокирован.");

    public static readonly Error UsernameTaken = Error.Conflict(
        "User.UsernameTaken",
        en: "This username is already taken. Please choose another one.",
        uz: "Bu username band. Boshqasini tanlang.",
        ru: "Это имя пользователя уже занято. Выберите другое.");

    public static readonly Error EmailTaken = Error.Conflict(
        "User.EmailTaken",
        en: "An account with this email already exists.",
        uz: "Bu email bilan hisob allaqachon mavjud.",
        ru: "Аккаунт с этим email уже существует.");

    public static readonly Error InvalidUsername = Error.Problem(
        "User.InvalidUsername",
        en: "Username may contain only Latin letters, digits and the characters . _ - @",
        uz: "Username faqat lotin harflari, raqamlar va . _ - @ belgilaridan iborat bo'lishi mumkin.",
        ru: "Имя пользователя может содержать только латинские буквы, цифры и символы . _ - @");

    public static readonly Error InvalidRole = Error.Problem(
        "User.InvalidRole",
        en: "The specified role does not exist.",
        uz: "Ko'rsatilgan rol mavjud emas.",
        ru: "Указанная роль не существует.");

    public static readonly Error CannotBlockSelf = Error.Problem(
        "User.CannotBlockSelf",
        en: "An administrator cannot block their own account.",
        uz: "Administrator o'z hisobini bloklay olmaydi.",
        ru: "Администратор не может заблокировать собственную учётную запись.");
}
