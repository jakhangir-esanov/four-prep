namespace FourPrep.Modules.Admission.Application.AcademyApplications.DeclineAcademyApplication;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Admission.Application.Abstractions.Data;
using FourPrep.Modules.Admission.Domain.AcademyApplications;
using FourPrep.Modules.Admission.Application.Abstractions.Authentication;

public sealed record DeclineAcademyApplicationCommand(Guid Id, string Reason) : ICommand;

internal sealed class DeclineAcademyApplicationCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IAcademyApplicationRepository repository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<DeclineAcademyApplicationCommand>
{
    public async Task<Result> Handle(DeclineAcademyApplicationCommand request, CancellationToken cancellationToken)
    {
        var application = await repository.GetAsync(request.Id, cancellationToken);
        if (application is null)
        {
            return Result.Failure(AcademyApplicationErrors.NotFound);
        }

        var declined = application.Decline(currentUser.UserId, request.Reason, dateTimeProvider.UtcNow);
        if (declined.IsFailure)
        {
            return declined;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
