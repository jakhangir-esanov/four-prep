namespace FourPrep.Modules.Academy.Domain.ClassGroups;

using FourPrep.Common.Domain;

public static class ClassGroupErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "ClassGroup.NotFound",
        en: "The specified group was not found.",
        uz: "Ko'rsatilgan guruh topilmadi.",
        ru: "Указанная группа не найдена.");

    public static readonly Error CodeTaken = Error.Conflict(
        "ClassGroup.CodeTaken",
        en: "Another group already uses this code. Choose a different one.",
        uz: "Bu kod boshqa guruhga berilgan. Boshqasini tanlang.",
        ru: "Этот код уже занят другой группой. Выберите другой.");

    public static readonly Error InvalidCode = Error.Problem(
        "ClassGroup.InvalidCode",
        en: "The group code may contain only capital Latin letters, digits and single hyphens, for example SAT-2026-OCT.",
        uz: "Guruh kodida faqat katta lotin harflari, raqamlar va yakka chiziqcha bo'lishi mumkin, masalan SAT-2026-OCT.",
        ru: "Код группы может содержать только заглавные латинские буквы, цифры и одиночные дефисы, например SAT-2026-OCT.");

    public static readonly Error OfficeLocationRequired = Error.Problem(
        "ClassGroup.OfficeLocationRequired",
        en: "An office group needs the country, city and address where the lessons take place.",
        uz: "Ofisdagi guruh uchun darslar o'tadigan davlat, shahar va manzil ko'rsatilishi kerak.",
        ru: "Для очной группы нужно указать страну, город и адрес, где проходят занятия.");

    public static readonly Error ScheduleRequired = Error.Problem(
        "ClassGroup.ScheduleRequired",
        en: "Add at least one lesson to the weekly schedule.",
        uz: "Haftalik jadvalga kamida bitta dars qo'shing.",
        ru: "Добавьте в недельное расписание хотя бы одно занятие.");

    public static readonly Error InvalidStatusChange = Error.Conflict(
        "ClassGroup.InvalidStatusChange",
        en: "The group cannot move to this status from its current one.",
        uz: "Guruhni hozirgi holatidan bu holatga o'tkazib bo'lmaydi.",
        ru: "Группу нельзя перевести в этот статус из текущего.");

    public static readonly Error Cancelled = Error.Conflict(
        "ClassGroup.Cancelled",
        en: "The group has been cancelled and can no longer be changed.",
        uz: "Guruh bekor qilingan, uni endi o'zgartirib bo'lmaydi.",
        ru: "Группа отменена, её больше нельзя изменить.");

    public static readonly Error NotDraft = Error.Conflict(
        "ClassGroup.NotDraft",
        en: "Only a draft group can be deleted. Cancel a group that has already been published.",
        uz: "Faqat qoralama guruhni o'chirish mumkin. E'lon qilingan guruhni bekor qiling.",
        ru: "Удалить можно только черновик группы. Опубликованную группу отмените.");

    public static readonly Error NotATeacher = Error.Problem(
        "ClassGroup.NotATeacher",
        en: "The specified user is not a teacher.",
        uz: "Ko'rsatilgan foydalanuvchi o'qituvchi emas.",
        ru: "Указанный пользователь не является преподавателем.");

    public static readonly Error NoTeacher = Error.Conflict(
        "ClassGroup.NoTeacher",
        en: "The group has no teacher assigned.",
        uz: "Guruhga o'qituvchi biriktirilmagan.",
        ru: "Группе не назначен преподаватель.");
}
