namespace FourPrep.Modules.Student.Application.StudentParents.LinkStudent;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Student.Domain.Students;
using FourPrep.Modules.Student.Domain.StudentParents;
using FourPrep.Modules.Student.Application.Abstractions.Data;
using FourPrep.Modules.Student.Application.Abstractions.LinkCodes;
using FourPrep.Modules.Student.Application.Abstractions.Authentication;

public sealed record LinkStudentCommand(string LinkCode) : ICommand<Guid>;

internal sealed class LinkStudentCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IStudentRepository studentRepository,
    IStudentParentRepository studentParentRepository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<LinkStudentCommand, Guid>
{
    public async Task<Result<Guid>> Handle(LinkStudentCommand request, CancellationToken cancellationToken)
    {
        var student = await studentRepository.GetByLinkCodeAsync(
            LinkCodeFormat.Normalize(request.LinkCode),
            cancellationToken);

        if (student is null)
        {
            return Result.Failure<Guid>(StudentErrors.LinkCodeNotFound);
        }

        if (await studentParentRepository.GetAsync(student.Id, currentUser.UserId, cancellationToken) is not null)
        {
            return Result.Failure<Guid>(StudentParentErrors.AlreadyLinked);
        }

        if (await studentParentRepository.CountByStudentIdAsync(student.Id, cancellationToken)
            >= StudentParent.MaxParentsPerStudent)
        {
            return Result.Failure<Guid>(StudentParentErrors.TooManyParents);
        }

        studentParentRepository.Insert(StudentParent.Create(
            student.Id,
            currentUser.UserId,
            currentUser.UserName,
            dateTimeProvider.UtcNow));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return student.Id;
    }
}
