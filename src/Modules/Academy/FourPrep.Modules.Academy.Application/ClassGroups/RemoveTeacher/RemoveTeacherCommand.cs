namespace FourPrep.Modules.Academy.Application.ClassGroups.RemoveTeacher;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Academy.Domain.ClassGroups;
using FourPrep.Modules.Academy.Application.Abstractions.Data;

public sealed record RemoveTeacherCommand(Guid ClassGroupId) : ICommand;

internal sealed class RemoveTeacherCommandHandler(
    IUnitOfWork unitOfWork,
    IClassGroupRepository repository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<RemoveTeacherCommand>
{
    public async Task<Result> Handle(RemoveTeacherCommand request, CancellationToken cancellationToken)
    {
        var classGroup = await repository.GetAsync(request.ClassGroupId, cancellationToken);
        if (classGroup is null)
        {
            return Result.Failure(ClassGroupErrors.NotFound);
        }

        if (classGroup.IsCancelled)
        {
            return Result.Failure(ClassGroupErrors.Cancelled);
        }

        if (classGroup.TeacherUserId is null)
        {
            return Result.Failure(ClassGroupErrors.NoTeacher);
        }

        classGroup.RemoveTeacher(dateTimeProvider.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
