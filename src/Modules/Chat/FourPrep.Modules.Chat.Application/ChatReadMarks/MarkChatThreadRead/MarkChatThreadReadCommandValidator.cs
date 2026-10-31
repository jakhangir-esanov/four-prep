namespace FourPrep.Modules.Chat.Application.ChatReadMarks.MarkChatThreadRead;

using FluentValidation;

internal sealed class MarkChatThreadReadCommandValidator : AbstractValidator<MarkChatThreadReadCommand>
{
    public MarkChatThreadReadCommandValidator()
    {
        RuleFor(x => x.StudentUserId).NotEmpty();
        RuleFor(x => x.Kind).IsInEnum();
        RuleFor(x => x.Audience).IsInEnum();
    }
}
