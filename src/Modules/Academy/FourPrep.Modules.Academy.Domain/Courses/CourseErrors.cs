namespace FourPrep.Modules.Academy.Domain.Courses;

using FourPrep.Common.Domain;

public static class CourseErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Course.NotFound",
        en: "The specified course was not found.",
        uz: "Ko'rsatilgan kurs topilmadi.",
        ru: "Указанный курс не найден.");

    public static readonly Error SlugTaken = Error.Conflict(
        "Course.SlugTaken",
        en: "Another course already uses this address name. Choose a different one.",
        uz: "Bu manzil nomi boshqa kursga berilgan. Boshqasini tanlang.",
        ru: "Это имя адреса уже занято другим курсом. Выберите другое.");

    public static readonly Error InvalidSlug = Error.Problem(
        "Course.InvalidSlug",
        en: "The address name may contain only lowercase Latin letters, digits and single hyphens, for example sat or general-english.",
        uz: "Manzil nomida faqat kichik lotin harflari, raqamlar va yakka chiziqcha bo'lishi mumkin, masalan sat yoki general-english.",
        ru: "Имя адреса может содержать только строчные латинские буквы, цифры и одиночные дефисы, например sat или general-english.");

    public static readonly Error NameRequired = Error.Problem(
        "Course.NameRequired",
        en: "The course needs a name, at least in English.",
        uz: "Kursga nom berilishi kerak, kamida ingliz tilida.",
        ru: "У курса должно быть название, как минимум на английском.");

    public static readonly Error HasClassGroups = Error.Conflict(
        "Course.HasClassGroups",
        en: "This course still has groups. Delete or move them before deleting the course.",
        uz: "Bu kursda hali guruhlar bor. Kursni o'chirishdan oldin guruhlarni o'chiring.",
        ru: "У этого курса ещё есть группы. Удалите их, прежде чем удалять курс.");
}
