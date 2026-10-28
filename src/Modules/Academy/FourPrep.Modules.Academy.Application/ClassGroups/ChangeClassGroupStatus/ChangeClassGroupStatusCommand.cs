namespace FourPrep.Modules.Academy.Application.ClassGroups.ChangeClassGroupStatus;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Academy.Domain.ClassGroups;
using FourPrep.Modules.Academy.Application.Abstractions.Data;

public sealed record ChangeClassGroupStatusCommand(Guid Id, ClassGroupStatus Status) : ICommand;

internal sealed class ChangeClassGroupStatusCommandHandler(
    IUnitOfWork unitOfWork,
    IClassGroupRepository repository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<ChangeClassGroupStatusCommand>
{
    public async Task<Result> Handle(ChangeClassGroupStatusCommand request, CancellationToken cancellationToken)
    {
        var classGroup = await repository.GetAsync(request.Id, cancellationToken);
        if (classGroup is null)
        {
            return Result.Failure(ClassGroupErrors.NotFound);
        }

        var changed = classGroup.ChangeStatus(request.Status, dateTimeProvider.UtcNow);
        if (changed.IsFailure)
        {
            return changed;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
