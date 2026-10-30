namespace FourPrep.Modules.Meeting.Application.Meetings.ChangeMeetingLink;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Meeting.Domain.Meetings;
using FourPrep.Modules.Meeting.Application.Abstractions.Data;
using FourPrep.Modules.Meeting.Application.Abstractions.Authentication;

public sealed record ChangeMeetingLinkCommand(Guid Id, string MeetingUrl) : ICommand;

internal sealed class ChangeMeetingLinkCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IMeetingRepository repository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<ChangeMeetingLinkCommand>
{
    public async Task<Result> Handle(ChangeMeetingLinkCommand request, CancellationToken cancellationToken)
    {
        var meeting = await repository.GetAsync(request.Id, cancellationToken);
        if (meeting is null || meeting.StaffUserId != currentUser.UserId)
        {
            return Result.Failure(MeetingErrors.NotFound);
        }

        var changed = meeting.ChangeLink(request.MeetingUrl.Trim(), dateTimeProvider.UtcNow);
        if (changed.IsFailure)
        {
            return changed;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
