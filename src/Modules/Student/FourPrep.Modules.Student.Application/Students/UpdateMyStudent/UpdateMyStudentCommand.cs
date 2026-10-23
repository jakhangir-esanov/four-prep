namespace FourPrep.Modules.Student.Application.Students.UpdateMyStudent;

using FourPrep.Common.Domain;
using FourPrep.Common.Domain.Enums;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Student.Domain.Students;
using FourPrep.Modules.Student.Domain.ExamGoals;
using FourPrep.Modules.Student.Application.Abstractions.Data;
using FourPrep.Modules.Student.Application.Abstractions.Authentication;

public sealed record ExamGoalInput(Exam Exam, decimal TargetScore);

public sealed record UpdateMyStudentCommand(
    string? FirstName,
    string? LastName,
    DateOnly? DateOfBirth,
    string? School,
    int? Grade,
    Region? Region,
    IReadOnlyList<ExamGoalInput> ExamGoals) : ICommand;

internal sealed class UpdateMyStudentCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IStudentRepository repository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<UpdateMyStudentCommand>
{
    public async Task<Result> Handle(UpdateMyStudentCommand request, CancellationToken cancellationToken)
    {
        var student = await repository.GetByUserIdAsync(currentUser.UserId, cancellationToken);
        if (student is null)
        {
            return Result.Failure(StudentErrors.NotFound);
        }

        student.UpdateProfile(
            Trimmed(request.FirstName),
            Trimmed(request.LastName),
            request.DateOfBirth,
            Trimmed(request.School),
            request.Grade,
            request.Region,
            request.ExamGoals.Select(goal => ExamGoal.Create(goal.Exam, goal.TargetScore)),
            dateTimeProvider.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static string? Trimmed(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
