namespace FourPrep.Modules.Chat.Application.ChatMessages.SendChatMessage;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Chat.Domain.ChatThreads;
using FourPrep.Modules.Chat.Domain.ChatMessages;
using FourPrep.Modules.Chat.Application.ChatThreads;
using FourPrep.Modules.Chat.Application.Abstractions.Data;
using FourPrep.Modules.Chat.Application.Abstractions.Students;
using FourPrep.Modules.Chat.Application.Abstractions.Authentication;

public sealed record SendChatMessageCommand(
    Guid StudentUserId,
    ChatThreadKind Kind,
    string Body,
    ChatAudience Audience = ChatAudience.Family) : ICommand<Guid>;

internal sealed class SendChatMessageCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IStudentService studentService,
    IChatThreadRepository threadRepository,
    IChatMessageRepository messageRepository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<SendChatMessageCommand, Guid>
{
    public async Task<Result<Guid>> Handle(SendChatMessageCommand request, CancellationToken cancellationToken)
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
            return Result.Failure<Guid>(student.Error);
        }

        if (request.Kind == ChatThreadKind.Mentor && student.Data.MentorUserId is null)
        {
            return Result.Failure<Guid>(ChatThreadErrors.NoMentor);
        }

        var utcNow = dateTimeProvider.UtcNow;

        var thread = await threadRepository.GetAsync(request.StudentUserId, request.Kind, cancellationToken);
        if (thread is null)
        {
            thread = ChatThread.Create(request.StudentUserId, request.Kind, student.Data.DisplayName, utcNow);
            threadRepository.Insert(thread);
        }
        else
        {
            thread.RenameStudent(student.Data.DisplayName, utcNow);
        }

        var message = ChatMessage.Create(
            thread.Id,
            currentUser.UserId,
            SenderName(student.Data),
            request.Audience == ChatAudience.Family ? ChatMessageSender.Family : ChatMessageSender.Staff,
            request.Body,
            utcNow);
        messageRepository.Insert(message);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return message.Id;
    }

    private string SenderName(ChatStudent student) =>
        student.UserId == currentUser.UserId ? student.DisplayName : currentUser.UserName;
}
