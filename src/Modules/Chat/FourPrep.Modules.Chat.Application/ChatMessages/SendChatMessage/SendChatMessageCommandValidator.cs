namespace FourPrep.Modules.Chat.Application.ChatMessages.SendChatMessage;

using FluentValidation;
using FourPrep.Common.Application.Extensions;
using FourPrep.Modules.Chat.Domain.ChatMessages;

internal sealed class SendChatMessageCommandValidator : AbstractValidator<SendChatMessageCommand>
{
    private const int MaxBodyLength = 4000;

    public SendChatMessageCommandValidator()
    {
        RuleFor(x => x.StudentUserId).NotEmpty();
        RuleFor(x => x.Kind).IsInEnum();
        RuleFor(x => x.Audience).IsInEnum();

        RuleFor(x => x.Body)
            .Must(body => !string.IsNullOrWhiteSpace(body) && body.Trim().Length <= MaxBodyLength)
            .WithError(ChatMessageErrors.BodyRequired);
    }
}
