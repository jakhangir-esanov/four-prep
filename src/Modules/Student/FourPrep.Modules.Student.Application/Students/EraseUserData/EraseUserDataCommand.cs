namespace FourPrep.Modules.Student.Application.Students.EraseUserData;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Student.Domain.Students;
using FourPrep.Modules.Student.Domain.StudentParents;
using FourPrep.Modules.Student.Application.Abstractions.Data;

public sealed record EraseUserDataCommand(Guid UserId) : ICommand;

internal sealed class EraseUserDataCommandHandler(
    IUnitOfWork unitOfWork,
    IStudentRepository studentRepository,
    IStudentParentRepository studentParentRepository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<EraseUserDataCommand>
{
    public async Task<Result> Handle(EraseUserDataCommand request, CancellationToken cancellationToken)
    {
        var student = await studentRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (student is not null)
        {
            await studentParentRepository.RemoveAllByStudentIdAsync(student.Id, cancellationToken);
            studentRepository.Remove(student);
        }

        await studentParentRepository.RemoveAllByParentUserIdAsync(request.UserId, cancellationToken);
        await studentRepository.ClearMentorAsync(request.UserId, dateTimeProvider.UtcNow, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
