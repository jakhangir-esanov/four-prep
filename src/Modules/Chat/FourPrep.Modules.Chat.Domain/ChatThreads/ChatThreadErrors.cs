namespace FourPrep.Modules.Chat.Domain.ChatThreads;

using FourPrep.Common.Domain;

public static class ChatThreadErrors
{
    public static readonly Error NotYourStudent = Error.Forbidden(
        "ChatThread.NotYourStudent",
        en: "You can open only the chats of your own account, of a child linked to you, or of a student you mentor.",
        uz: "Faqat o'zingizning, sizga bog'langan farzandingizning yoki o'zingiz mentorlik qiladigan o'quvchining yozishmalarini ochishingiz mumkin.",
        ru: "Открывать можно только свою переписку, переписку привязанного к вам ребёнка или ученика, которого вы ведёте как ментор.");

    public static readonly Error NoMentor = Error.Conflict(
        "ChatThread.NoMentor",
        en: "This student has no mentor yet. Write to support until a mentor is assigned.",
        uz: "Bu o'quvchiga hali mentor biriktirilmagan. Mentor tayinlanguncha support'ga yozing.",
        ru: "У этого ученика пока нет ментора. Пока его не назначат, пишите в поддержку.");
}
