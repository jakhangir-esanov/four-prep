namespace FourPrep.Modules.Student.Application.Students.RemoveMentor;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Student.Domain.Students;
using FourPrep.Modules.Student.Application.Abstractions.Data;

public sealed record RemoveMentorCommand(Guid StudentId) : ICommand;

internal sealed class RemoveMentorCommandHandler(
    IUnitOfWork unitOfWork,
    IStudentRepository repository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<RemoveMentorCommand>
{
    public async Task<Result> Handle(RemoveMentorCommand request, CancellationToken cancellationToken)
    {
        var student = await repository.GetAsync(request.StudentId, cancellationToken);
        if (student is null)
        {
            return Result.Failure(StudentErrors.NotFound);
        }

        if (student.MentorUserId is null)
        {
            return Result.Failure(StudentErrors.NoMentor);
        }

        student.RemoveMentor(dateTimeProvider.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
