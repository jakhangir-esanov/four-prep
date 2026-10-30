namespace FourPrep.Modules.Meeting.Application.Meetings.StaffCancelMeeting;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Meeting.Domain.Meetings;
using FourPrep.Modules.Meeting.Application.Abstractions.Data;
using FourPrep.Modules.Meeting.Application.Abstractions.Authentication;

public sealed record StaffCancelMeetingCommand(Guid Id, string? Reason, bool AnyStaff = false) : ICommand;

internal sealed class StaffCancelMeetingCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IMeetingRepository repository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<StaffCancelMeetingCommand>
{
    public async Task<Result> Handle(StaffCancelMeetingCommand request, CancellationToken cancellationToken)
    {
        var meeting = await repository.GetAsync(request.Id, cancellationToken);
        if (meeting is null || (!request.AnyStaff && meeting.StaffUserId != currentUser.UserId))
        {
            return Result.Failure(MeetingErrors.NotFound);
        }

        var reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim();

        var cancelled = meeting.Cancel(currentUser.UserId, reason, dateTimeProvider.UtcNow);
        if (cancelled.IsFailure)
        {
            return cancelled;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
