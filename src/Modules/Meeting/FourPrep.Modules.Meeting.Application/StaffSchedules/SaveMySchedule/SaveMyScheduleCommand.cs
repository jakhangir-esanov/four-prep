namespace FourPrep.Modules.Meeting.Application.StaffSchedules.SaveMySchedule;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Meeting.Domain.StaffSchedules;
using FourPrep.Modules.Meeting.Application.Abstractions.Data;
using FourPrep.Modules.Meeting.Application.Abstractions.Authentication;

public sealed record SaveMyScheduleCommand(StaffScheduleDetails Details) : ICommand;

internal sealed class SaveMyScheduleCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IStaffScheduleRepository repository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<SaveMyScheduleCommand>
{
    public async Task<Result> Handle(SaveMyScheduleCommand request, CancellationToken cancellationToken)
    {
        var utcNow = dateTimeProvider.UtcNow;
        var schedule = await repository.GetByStaffUserIdAsync(currentUser.UserId, cancellationToken);

        if (schedule is null)
        {
            repository.Insert(request.Details.ToSchedule(currentUser.UserId, utcNow));
        }
        else
        {
            request.Details.ApplyTo(schedule, utcNow);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
