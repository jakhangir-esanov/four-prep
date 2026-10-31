namespace FourPrep.Modules.Chat.Application.ChatReadMarks.MarkChatThreadRead;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Chat.Domain.ChatThreads;
using FourPrep.Modules.Chat.Domain.ChatReadMarks;
using FourPrep.Modules.Chat.Application.ChatThreads;
using FourPrep.Modules.Chat.Application.Abstractions.Data;
using FourPrep.Modules.Chat.Application.Abstractions.Students;
using FourPrep.Modules.Chat.Application.Abstractions.Authentication;

public sealed record MarkChatThreadReadCommand(
    Guid StudentUserId,
    ChatThreadKind Kind,
    ChatAudience Audience = ChatAudience.Family) : ICommand;

internal sealed class MarkChatThreadReadCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IStudentService studentService,
    IChatThreadRepository threadRepository,
    IChatReadMarkRepository readMarkRepository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<MarkChatThreadReadCommand>
{
    public async Task<Result> Handle(MarkChatThreadReadCommand request, CancellationToken cancellationToken)
    {
        var student = await ChatAccess.CheckAsync(
            studentService,
            currentUser,
            request.StudentUserId,
            request.Kind,
            request.Audience,
            cancellationToken);
        if (student.IsFailure)
        {
            return student;
        }

        var thread = await threadRepository.GetAsync(request.StudentUserId, request.Kind, cancellationToken);
        if (thread is null)
        {
            return Result.Success();
        }

        var utcNow = dateTimeProvider.UtcNow;

        var readMark = await readMarkRepository.GetAsync(thread.Id, currentUser.UserId, cancellationToken);
        if (readMark is null)
        {
            readMarkRepository.Insert(ChatReadMark.Create(thread.Id, currentUser.UserId, utcNow));
        }
        else
        {
            readMark.MarkRead(utcNow);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
