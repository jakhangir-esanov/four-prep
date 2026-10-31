namespace FourPrep.Modules.Admission.Application.AcademyApplications.AcceptAcademyApplication;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Admission.Application.Abstractions.Data;
using FourPrep.Modules.Admission.Domain.AcademyApplications;
using FourPrep.Modules.Admission.Application.Abstractions.Authentication;

public sealed record AcceptAcademyApplicationCommand(Guid Id) : ICommand;

internal sealed class AcceptAcademyApplicationCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IAcademyApplicationRepository repository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<AcceptAcademyApplicationCommand>
{
    public async Task<Result> Handle(AcceptAcademyApplicationCommand request, CancellationToken cancellationToken)
    {
        var application = await repository.GetAsync(request.Id, cancellationToken);
        if (application is null)
        {
            return Result.Failure(AcademyApplicationErrors.NotFound);
        }

        var accepted = application.Accept(currentUser.UserId, dateTimeProvider.UtcNow);
        if (accepted.IsFailure)
        {
            return accepted;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
