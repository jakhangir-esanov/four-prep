namespace FourPrep.Modules.Student.Application.StudentParents.UnlinkStudent;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Student.Domain.StudentParents;
using FourPrep.Modules.Student.Application.Abstractions.Data;
using FourPrep.Modules.Student.Application.Abstractions.Authentication;

public sealed record UnlinkStudentCommand(Guid StudentId) : ICommand;

internal sealed class UnlinkStudentCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IStudentParentRepository repository) : ICommandHandler<UnlinkStudentCommand>
{
    public async Task<Result> Handle(UnlinkStudentCommand request, CancellationToken cancellationToken)
    {
        var link = await repository.GetAsync(request.StudentId, currentUser.UserId, cancellationToken);
        if (link is null)
        {
            return Result.Failure(StudentParentErrors.NotFound);
        }

        repository.Remove(link);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
