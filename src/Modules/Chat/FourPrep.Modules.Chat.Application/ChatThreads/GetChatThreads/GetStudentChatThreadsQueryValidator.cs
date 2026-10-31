namespace FourPrep.Modules.Chat.Application.ChatThreads.GetChatThreads;

using FluentValidation;

internal sealed class GetStudentChatThreadsQueryValidator : AbstractValidator<GetStudentChatThreadsQuery>
{
    public GetStudentChatThreadsQueryValidator()
    {
        RuleFor(x => x.StudentUserId).NotEmpty();
    }
}
