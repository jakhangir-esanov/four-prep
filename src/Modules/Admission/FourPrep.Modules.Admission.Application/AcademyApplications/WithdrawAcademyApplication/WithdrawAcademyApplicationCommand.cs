namespace FourPrep.Modules.Admission.Application.AcademyApplications.WithdrawAcademyApplication;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Admission.Application.Abstractions.Data;
using FourPrep.Modules.Admission.Domain.AcademyApplications;
using FourPrep.Modules.Admission.Application.Abstractions.Authentication;

public sealed record WithdrawAcademyApplicationCommand(Guid Id) : ICommand;

internal sealed class WithdrawAcademyApplicationCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IAcademyApplicationRepository repository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<WithdrawAcademyApplicationCommand>
{
    public async Task<Result> Handle(WithdrawAcademyApplicationCommand request, CancellationToken cancellationToken)
    {
        var application = await repository.GetAsync(request.Id, cancellationToken);
        if (application is null || application.StudentUserId != currentUser.UserId)
        {
            return Result.Failure(AcademyApplicationErrors.NotFound);
        }

        var withdrawn = application.Withdraw(dateTimeProvider.UtcNow);
        if (withdrawn.IsFailure)
        {
            return withdrawn;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
