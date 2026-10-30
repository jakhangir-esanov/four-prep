namespace FourPrep.Modules.Content.Domain.NewsPosts;

using FourPrep.Common.Domain;

public static class NewsPostErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "NewsPost.NotFound",
        en: "The specified news post was not found.",
        uz: "Ko'rsatilgan yangilik topilmadi.",
        ru: "Указанная новость не найдена.");

    public static readonly Error SlugTaken = Error.Conflict(
        "NewsPost.SlugTaken",
        en: "Another news post already uses this address name. Choose a different one.",
        uz: "Bu manzil nomi boshqa yangilikka berilgan. Boshqasini tanlang.",
        ru: "Это имя адреса уже занято другой новостью. Выберите другое.");

    public static readonly Error InvalidSlug = Error.Problem(
        "NewsPost.InvalidSlug",
        en: "The address name may contain only lowercase Latin letters, digits and single hyphens, for example ielts-results-2026.",
        uz: "Manzil nomida faqat kichik lotin harflari, raqamlar va yakka chiziqcha bo'lishi mumkin, masalan ielts-results-2026.",
        ru: "Имя адреса может содержать только строчные латинские буквы, цифры и одиночные дефисы, например ielts-results-2026.");

    public static readonly Error TitleRequired = Error.Problem(
        "NewsPost.TitleRequired",
        en: "The news post needs a title in at least one language.",
        uz: "Yangilikka kamida bitta tilda sarlavha yozilishi kerak.",
        ru: "У новости должен быть заголовок хотя бы на одном языке.");

    public static readonly Error TextLanguagesMismatch = Error.Problem(
        "NewsPost.TextLanguagesMismatch",
        en: "Write the title and the text in the same languages: every language with a title needs a text, and every text needs a title.",
        uz: "Sarlavha va matnni bir xil tillarda yozing: sarlavhasi bor har bir tilda matn, matni bor har bir tilda sarlavha bo'lishi kerak.",
        ru: "Пишите заголовок и текст на одних и тех же языках: у каждого языка с заголовком должен быть текст, а у каждого текста — заголовок.");

    public static readonly Error InvalidTag = Error.Problem(
        "NewsPost.InvalidTag",
        en: "The tag may contain only lowercase Latin letters, digits and single hyphens, for example admissions.",
        uz: "Tegda faqat kichik lotin harflari, raqamlar va yakka chiziqcha bo'lishi mumkin, masalan admissions.",
        ru: "Тег может содержать только строчные латинские буквы, цифры и одиночные дефисы, например admissions.");

    public static readonly Error AlreadyPublished = Error.Conflict(
        "NewsPost.AlreadyPublished",
        en: "This news post is already published.",
        uz: "Bu yangilik allaqachon e'lon qilingan.",
        ru: "Эта новость уже опубликована.");

    public static readonly Error NotPublished = Error.Conflict(
        "NewsPost.NotPublished",
        en: "This news post is not published.",
        uz: "Bu yangilik e'lon qilinmagan.",
        ru: "Эта новость не опубликована.");

    public static readonly Error CoverNotFound = Error.NotFound(
        "NewsPost.CoverNotFound",
        en: "The cover image was not found. Upload it yourself first, then attach it.",
        uz: "Muqova rasmi topilmadi. Avval uni o'zingiz yuklang, keyin biriktiring.",
        ru: "Изображение обложки не найдено. Сначала загрузите его сами, затем прикрепите.");

    public static readonly Error CoverNotAnImage = Error.Problem(
        "NewsPost.CoverNotAnImage",
        en: "The cover must be a JPG or PNG image.",
        uz: "Muqova JPG yoki PNG rasm bo'lishi kerak.",
        ru: "Обложка должна быть изображением JPG или PNG.");

    public static readonly Error CoverInUse = Error.Conflict(
        "NewsPost.CoverInUse",
        en: "This image is already the cover of another news post. Upload it again to use it here too.",
        uz: "Bu rasm boshqa yangilikning muqovasi. Bu yerda ham ishlatish uchun uni qaytadan yuklang.",
        ru: "Это изображение уже является обложкой другой новости. Загрузите его ещё раз, чтобы использовать и здесь.");
}
