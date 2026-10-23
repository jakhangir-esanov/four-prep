namespace FourPrep.Modules.Student.Application.Students.AssignMentor;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Domain.Authorization;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Student.Domain.Students;
using FourPrep.Modules.Student.Application.Abstractions.Auth;
using FourPrep.Modules.Student.Application.Abstractions.Data;

public sealed record AssignMentorCommand(Guid StudentId, Guid MentorUserId) : ICommand;

internal sealed class AssignMentorCommandHandler(
    IUnitOfWork unitOfWork,
    IAuthService authService,
    IStudentRepository repository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<AssignMentorCommand>
{
    public async Task<Result> Handle(AssignMentorCommand request, CancellationToken cancellationToken)
    {
        var student = await repository.GetAsync(request.StudentId, cancellationToken);
        if (student is null)
        {
            return Result.Failure(StudentErrors.NotFound);
        }

        var roles = await authService.GetUserRolesAsync(request.MentorUserId, cancellationToken);
        if (roles.IsFailure)
        {
            return Result.Failure(roles.Error);
        }

        if (!roles.Data.Contains(Roles.Mentor))
        {
            return Result.Failure(StudentErrors.NotAMentor);
        }

        student.AssignMentor(request.MentorUserId, dateTimeProvider.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
