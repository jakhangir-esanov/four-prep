namespace FourPrep.Modules.Academy.Application.ClassGroups.AssignTeacher;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Domain.Authorization;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Academy.Domain.ClassGroups;
using FourPrep.Modules.Academy.Application.Abstractions.Auth;
using FourPrep.Modules.Academy.Application.Abstractions.Data;

public sealed record AssignTeacherCommand(Guid ClassGroupId, Guid TeacherUserId, string TeacherName) : ICommand;

internal sealed class AssignTeacherCommandHandler(
    IUnitOfWork unitOfWork,
    IAuthService authService,
    IClassGroupRepository repository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<AssignTeacherCommand>
{
    public async Task<Result> Handle(AssignTeacherCommand request, CancellationToken cancellationToken)
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

        var roles = await authService.GetUserRolesAsync(request.TeacherUserId, cancellationToken);
        if (roles.IsFailure)
        {
            return Result.Failure(roles.Error);
        }

        if (!roles.Data.Contains(Roles.Teacher))
        {
            return Result.Failure(ClassGroupErrors.NotATeacher);
        }

        classGroup.AssignTeacher(request.TeacherUserId, request.TeacherName.Trim(), dateTimeProvider.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
