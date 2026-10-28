namespace FourPrep.Modules.Academy.Application.Enrolments.ActivateEnrolment;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Academy.Domain.ClassGroups;
using FourPrep.Modules.Academy.Domain.Enrolments;
using FourPrep.Modules.Academy.Application.Abstractions.Data;

public sealed record ActivateEnrolmentCommand(Guid EnrolmentId) : ICommand;

internal sealed class ActivateEnrolmentCommandHandler(
    IUnitOfWork unitOfWork,
    IClassGroupRepository repository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<ActivateEnrolmentCommand>
{
    public async Task<Result> Handle(ActivateEnrolmentCommand request, CancellationToken cancellationToken)
    {
        var classGroup = await repository.GetByEnrolmentIdAsync(request.EnrolmentId, cancellationToken);
        if (classGroup is null)
        {
            return Result.Failure(EnrolmentErrors.NotFound);
        }

        var activated = classGroup.ActivateEnrolment(request.EnrolmentId, dateTimeProvider.UtcNow);
        if (activated.IsFailure)
        {
            return activated;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
