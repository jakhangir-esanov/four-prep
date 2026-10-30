namespace FourPrep.Modules.Meeting.Application.Meetings.RecordAttendance;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Meeting.Domain.Meetings;
using FourPrep.Modules.Meeting.Application.Abstractions.Data;
using FourPrep.Modules.Meeting.Application.Abstractions.Authentication;

public sealed record RecordAttendanceCommand(Guid Id, MeetingStatus Status) : ICommand;

internal sealed class RecordAttendanceCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IMeetingRepository repository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<RecordAttendanceCommand>
{
    public async Task<Result> Handle(RecordAttendanceCommand request, CancellationToken cancellationToken)
    {
        var meeting = await repository.GetAsync(request.Id, cancellationToken);
        if (meeting is null || meeting.StaffUserId != currentUser.UserId)
        {
            return Result.Failure(MeetingErrors.NotFound);
        }

        var recorded = meeting.RecordAttendance(request.Status, dateTimeProvider.UtcNow);
        if (recorded.IsFailure)
        {
            return recorded;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
