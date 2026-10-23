namespace FourPrep.Modules.Student.Application.Students.CreateStudent;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Student.Domain.Students;
using FourPrep.Modules.Student.Application.Abstractions.Data;
using FourPrep.Modules.Student.Application.Abstractions.LinkCodes;

public sealed record CreateStudentCommand(Guid UserId, string UserName) : ICommand<Guid>;

internal sealed class CreateStudentCommandHandler(
    IUnitOfWork unitOfWork,
    IStudentRepository repository,
    ILinkCodeGenerator linkCodeGenerator,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<CreateStudentCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
    {
        var existing = await repository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (existing is not null)
        {
            return existing.Id;
        }

        var linkCode = await UniqueLinkCodes.NextAsync(linkCodeGenerator, repository, cancellationToken);
        if (linkCode.IsFailure)
        {
            return Result.Failure<Guid>(linkCode.Error);
        }

        var student = Student.Create(request.UserId, request.UserName, linkCode.Data, dateTimeProvider.UtcNow);

        repository.Insert(student);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return student.Id;
    }
}
