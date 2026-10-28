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

    public static readonly Error CannotDeleteSelf = Error.Problem(
        "User.CannotDeleteSelf",
        en: "An administrator cannot delete their own account.",
        uz: "Administrator o'z hisobini o'chira olmaydi.",
        ru: "Администратор не может удалить собственную учётную запись.");

    public static readonly Error WrongPassword = Error.Problem(
        "User.WrongPassword",
        en: "The password is incorrect.",
        uz: "Parol noto'g'ri.",
        ru: "Неверный пароль.");

    public static readonly Error PasswordNotSet = Error.Problem(
        "User.PasswordNotSet",
        en: "This account signs in with Google and has no password yet. Use \"Forgot password\" to create one.",
        uz: "Bu hisob Google orqali kiradi va hali paroli yo'q. Parol yaratish uchun \"Parolni unutdim\" dan foydalaning.",
        ru: "Этот аккаунт входит через Google, и у него ещё нет пароля. Чтобы создать пароль, воспользуйтесь «Забыли пароль».");

    public static readonly Error PasswordNotAllowed = Error.Problem(
        "User.PasswordNotAllowed",
        en: "Choose a different password: it must be 8 to 128 characters long and must not be your username or email address.",
        uz: "Boshqa parol tanlang: u 8 dan 128 belgigacha bo'lishi, username yoki email manzilingiz bilan bir xil bo'lmasligi kerak.",
        ru: "Выберите другой пароль: от 8 до 128 символов, не совпадающий с вашим логином или адресом электронной почты.");

    public static readonly Error PasswordUnchanged = Error.Problem(
        "User.PasswordUnchanged",
        en: "The new password must be different from the current one.",
        uz: "Yangi parol hozirgisidan farq qilishi kerak.",
        ru: "Новый пароль должен отличаться от текущего.");

    public static readonly Error ReauthenticationRequired = Error.Problem(
        "User.ReauthenticationRequired",
        en: "For your security, sign in again and then repeat this action within 10 minutes.",
        uz: "Xavfsizlik uchun qaytadan kiring va bu amalni 10 daqiqa ichida takrorlang.",
        ru: "В целях безопасности войдите снова и повторите это действие в течение 10 минут.");

    public static readonly Error EmailAlreadyVerified = Error.Conflict(
        "User.EmailAlreadyVerified",
        en: "This email address is already verified.",
        uz: "Bu email manzil allaqachon tasdiqlangan.",
        ru: "Этот адрес электронной почты уже подтверждён.");
}
