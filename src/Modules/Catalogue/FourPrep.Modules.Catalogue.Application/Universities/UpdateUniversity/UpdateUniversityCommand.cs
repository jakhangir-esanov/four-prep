namespace FourPrep.Modules.Catalogue.Application.Universities.UpdateUniversity;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Catalogue.Domain.Universities;
using FourPrep.Modules.Catalogue.Application.Abstractions.Data;
using FourPrep.Modules.Catalogue.Application.Abstractions.Authentication;

public sealed record UpdateUniversityCommand(Guid Id, UniversityDetails Details) : ICommand;

internal sealed class UpdateUniversityCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IUniversityRepository repository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<UpdateUniversityCommand>
{
    public async Task<Result> Handle(UpdateUniversityCommand request, CancellationToken cancellationToken)
    {
        var university = await repository.GetAsync(request.Id, cancellationToken);
        if (university is null)
        {
            return Result.Failure(UniversityErrors.NotFound);
        }

        if (await repository.IsSlugTakenAsync(request.Details.NormalizedSlug, university.Id, cancellationToken))
        {
            return Result.Failure(UniversityErrors.SlugTaken);
        }

        request.Details.ApplyTo(university, currentUser.UserId, dateTimeProvider.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
