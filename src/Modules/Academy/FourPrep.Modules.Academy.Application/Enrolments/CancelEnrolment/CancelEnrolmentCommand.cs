namespace FourPrep.Modules.Academy.Application.Enrolments.CancelEnrolment;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Academy.Domain.ClassGroups;
using FourPrep.Modules.Academy.Domain.Enrolments;
using FourPrep.Modules.Academy.Application.Abstractions.Data;
using FourPrep.Modules.Academy.Application.Abstractions.Students;
using FourPrep.Modules.Academy.Application.Abstractions.Authentication;

public sealed record CancelEnrolmentCommand(Guid EnrolmentId) : ICommand;

internal sealed class CancelEnrolmentCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IStudentService studentService,
    IClassGroupRepository repository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<CancelEnrolmentCommand>
{
    public async Task<Result> Handle(CancelEnrolmentCommand request, CancellationToken cancellationToken)
    {
        var classGroup = await repository.GetByEnrolmentIdAsync(request.EnrolmentId, cancellationToken);
        if (classGroup is null)
        {
            return Result.Failure(EnrolmentErrors.NotFound);
        }

        var studentUserId = classGroup.Enrolments.Single(x => x.Id == request.EnrolmentId).StudentUserId;

        var access = await FamilyAccess.CheckAsync(studentService, currentUser.UserId, studentUserId, cancellationToken);
        if (access.IsFailure)
        {
            return access.Error == EnrolmentErrors.NotYourStudent ? Result.Failure(EnrolmentErrors.NotFound) : access;
        }

        var cancelled = classGroup.CancelEnrolment(request.EnrolmentId, dateTimeProvider.UtcNow);
        if (cancelled.IsFailure)
        {
            return cancelled;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
