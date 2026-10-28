namespace FourPrep.Modules.Academy.Application.Enrolments.RemoveEnrolment;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Academy.Domain.ClassGroups;
using FourPrep.Modules.Academy.Domain.Enrolments;
using FourPrep.Modules.Academy.Application.Abstractions.Data;

public sealed record RemoveEnrolmentCommand(Guid EnrolmentId) : ICommand;

internal sealed class RemoveEnrolmentCommandHandler(
    IUnitOfWork unitOfWork,
    IClassGroupRepository repository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<RemoveEnrolmentCommand>
{
    public async Task<Result> Handle(RemoveEnrolmentCommand request, CancellationToken cancellationToken)
    {
        var classGroup = await repository.GetByEnrolmentIdAsync(request.EnrolmentId, cancellationToken);
        if (classGroup is null)
        {
            return Result.Failure(EnrolmentErrors.NotFound);
        }

        var removed = classGroup.RemoveEnrolment(request.EnrolmentId, dateTimeProvider.UtcNow);
        if (removed.IsFailure)
        {
            return removed;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
