namespace FourPrep.Modules.Academy.Application.ClassGroups.UpdateClassGroup;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Academy.Domain.ClassGroups;
using FourPrep.Modules.Academy.Application.Abstractions.Data;

public sealed record UpdateClassGroupCommand(Guid Id, ClassGroupDetails Details) : ICommand;

internal sealed class UpdateClassGroupCommandHandler(
    IUnitOfWork unitOfWork,
    IClassGroupRepository repository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<UpdateClassGroupCommand>
{
    public async Task<Result> Handle(UpdateClassGroupCommand request, CancellationToken cancellationToken)
    {
        var classGroup = await repository.GetAsync(request.Id, cancellationToken);
        if (classGroup is null)
        {
            return Result.Failure(ClassGroupErrors.NotFound);
        }

        if (classGroup.IsCancelled)
        {
            return Result.Failure(ClassGroupErrors.Cancelled);
        }

        if (await repository.IsCodeTakenAsync(request.Details.NormalizedCode, classGroup.Id, cancellationToken))
        {
            return Result.Failure(ClassGroupErrors.CodeTaken);
        }

        request.Details.ApplyTo(classGroup, dateTimeProvider.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
