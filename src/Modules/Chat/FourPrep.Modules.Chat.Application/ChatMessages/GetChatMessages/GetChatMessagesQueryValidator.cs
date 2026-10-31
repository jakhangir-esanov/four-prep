namespace FourPrep.Modules.Chat.Application.ChatMessages.GetChatMessages;

using FluentValidation;

internal sealed class GetChatMessagesQueryValidator : AbstractValidator<GetChatMessagesQuery>
{
    public GetChatMessagesQueryValidator()
    {
        RuleFor(x => x.StudentUserId).NotEmpty();
        RuleFor(x => x.Kind).IsInEnum();
        RuleFor(x => x.Audience).IsInEnum();
    }
}
