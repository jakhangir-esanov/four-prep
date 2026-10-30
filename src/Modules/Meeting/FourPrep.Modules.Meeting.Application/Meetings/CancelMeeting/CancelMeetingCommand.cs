namespace FourPrep.Modules.Meeting.Application.Meetings.CancelMeeting;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Meeting.Domain.Meetings;
using FourPrep.Modules.Meeting.Application.Abstractions.Data;
using FourPrep.Modules.Meeting.Application.Abstractions.Students;
using FourPrep.Modules.Meeting.Application.Abstractions.Authentication;

public sealed record CancelMeetingCommand(Guid Id) : ICommand;

internal sealed class CancelMeetingCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IStudentService studentService,
    IMeetingRepository repository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<CancelMeetingCommand>
{
    public async Task<Result> Handle(CancelMeetingCommand request, CancellationToken cancellationToken)
    {
        var meeting = await repository.GetAsync(request.Id, cancellationToken);
        if (meeting is null)
        {
            return Result.Failure(MeetingErrors.NotFound);
        }

        var access = await studentService.HasFamilyAccessAsync(currentUser.UserId, meeting.StudentUserId, cancellationToken);
        if (access.IsFailure || !access.Data)
        {
            return Result.Failure(MeetingErrors.NotFound);
        }

        var utcNow = dateTimeProvider.UtcNow;
        if (meeting.Status == MeetingStatus.Scheduled && meeting.HasStarted(utcNow))
        {
            return Result.Failure(MeetingErrors.AlreadyStarted);
        }

        var cancelled = meeting.Cancel(currentUser.UserId, null, utcNow);
        if (cancelled.IsFailure)
        {
            return cancelled;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
