namespace FourPrep.Modules.Student.Application.StudentParents.RemoveParent;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Student.Domain.Students;
using FourPrep.Modules.Student.Domain.StudentParents;
using FourPrep.Modules.Student.Application.Abstractions.Data;
using FourPrep.Modules.Student.Application.Abstractions.Authentication;

public sealed record RemoveParentCommand(Guid ParentUserId) : ICommand;

internal sealed class RemoveParentCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IStudentRepository studentRepository,
    IStudentParentRepository studentParentRepository) : ICommandHandler<RemoveParentCommand>
{
    public async Task<Result> Handle(RemoveParentCommand request, CancellationToken cancellationToken)
    {
        var student = await studentRepository.GetByUserIdAsync(currentUser.UserId, cancellationToken);
        if (student is null)
        {
            return Result.Failure(StudentErrors.NotFound);
        }

        var link = await studentParentRepository.GetAsync(student.Id, request.ParentUserId, cancellationToken);
        if (link is null)
        {
            return Result.Failure(StudentParentErrors.NotFound);
        }

        studentParentRepository.Remove(link);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
