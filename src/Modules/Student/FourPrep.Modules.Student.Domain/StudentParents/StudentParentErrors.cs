namespace FourPrep.Modules.Student.Domain.StudentParents;

using FourPrep.Common.Domain;

public static class StudentParentErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "StudentParent.NotFound",
        en: "This parent is not linked to the student.",
        uz: "Bu ota-ona o'quvchiga bog'lanmagan.",
        ru: "Этот родитель не привязан к ученику.");

    public static readonly Error AlreadyLinked = Error.Conflict(
        "StudentParent.AlreadyLinked",
        en: "You are already linked to this student.",
        uz: "Siz bu o'quvchiga allaqachon bog'langansiz.",
        ru: "Вы уже привязаны к этому ученику.");

    public static readonly Error TooManyParents = Error.Conflict(
        "StudentParent.TooManyParents",
        en: "This student already has the maximum number of linked parents.",
        uz: "Bu o'quvchiga bog'lanishi mumkin bo'lgan ota-onalar soni to'lgan.",
        ru: "К этому ученику уже привязано максимальное число родителей.");
}
