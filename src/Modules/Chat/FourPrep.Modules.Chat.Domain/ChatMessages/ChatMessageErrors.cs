namespace FourPrep.Modules.Chat.Domain.ChatMessages;

using FourPrep.Common.Domain;

public static class ChatMessageErrors
{
    public static readonly Error BodyRequired = Error.Problem(
        "ChatMessage.BodyRequired",
        en: "Write a message of at most 4000 characters.",
        uz: "Ko'pi bilan 4000 belgidan iborat xabar yozing.",
        ru: "Напишите сообщение длиной не более 4000 символов.");
}
