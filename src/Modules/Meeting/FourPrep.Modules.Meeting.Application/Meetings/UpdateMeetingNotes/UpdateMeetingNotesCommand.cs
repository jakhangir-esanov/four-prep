namespace FourPrep.Modules.Meeting.Application.Meetings.UpdateMeetingNotes;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Meeting.Domain.Meetings;
using FourPrep.Modules.Meeting.Application.Abstractions.Data;
using FourPrep.Modules.Meeting.Application.Abstractions.Authentication;

public sealed record UpdateMeetingNotesCommand(Guid Id, string? Notes) : ICommand;

internal sealed class UpdateMeetingNotesCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IMeetingRepository repository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<UpdateMeetingNotesCommand>
{
    public async Task<Result> Handle(UpdateMeetingNotesCommand request, CancellationToken cancellationToken)
    {
        var meeting = await repository.GetAsync(request.Id, cancellationToken);
        if (meeting is null || meeting.StaffUserId != currentUser.UserId)
        {
            return Result.Failure(MeetingErrors.NotFound);
        }

        meeting.UpdateNotes(string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(), dateTimeProvider.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
