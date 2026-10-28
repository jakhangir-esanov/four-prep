namespace FourPrep.Modules.Academy.Application.ClassGroups.DeleteClassGroup;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Academy.Domain.ClassGroups;
using FourPrep.Modules.Academy.Application.Abstractions.Data;

public sealed record DeleteClassGroupCommand(Guid Id) : ICommand;

internal sealed class DeleteClassGroupCommandHandler(
    IUnitOfWork unitOfWork,
    IClassGroupRepository repository) : ICommandHandler<DeleteClassGroupCommand>
{
    public async Task<Result> Handle(DeleteClassGroupCommand request, CancellationToken cancellationToken)
    {
        var classGroup = await repository.GetAsync(request.Id, cancellationToken);
        if (classGroup is null)
        {
            return Result.Failure(ClassGroupErrors.NotFound);
        }

        if (classGroup.Status != ClassGroupStatus.Draft)
        {
            return Result.Failure(ClassGroupErrors.NotDraft);
        }

        repository.Remove(classGroup);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
