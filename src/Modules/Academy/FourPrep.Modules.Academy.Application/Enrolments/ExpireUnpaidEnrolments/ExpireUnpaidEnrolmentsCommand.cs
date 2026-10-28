namespace FourPrep.Modules.Academy.Application.Enrolments.ExpireUnpaidEnrolments;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Academy.Domain.ClassGroups;
using FourPrep.Modules.Academy.Application.Abstractions.Data;

public sealed record ExpireUnpaidEnrolmentsCommand : ICommand<int>;

internal sealed class ExpireUnpaidEnrolmentsCommandHandler(
    IUnitOfWork unitOfWork,
    IClassGroupRepository repository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<ExpireUnpaidEnrolmentsCommand, int>
{
    public async Task<Result<int>> Handle(ExpireUnpaidEnrolmentsCommand request, CancellationToken cancellationToken)
    {
        var utcNow = dateTimeProvider.UtcNow;
        var classGroups = await repository.GetWithPaymentsDueAsync(utcNow, cancellationToken);

        var expired = classGroups.Sum(classGroup => classGroup.ExpireUnpaidEnrolments(utcNow));

        if (expired > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return expired;
    }
}
