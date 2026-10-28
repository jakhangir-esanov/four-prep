namespace FourPrep.Modules.Catalogue.Application.Universities.CreateUniversity;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Catalogue.Domain.Universities;
using FourPrep.Modules.Catalogue.Application.Abstractions.Data;
using FourPrep.Modules.Catalogue.Application.Abstractions.Authentication;

public sealed record CreateUniversityCommand(UniversityDetails Details) : ICommand<Guid>;

internal sealed class CreateUniversityCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IUniversityRepository repository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<CreateUniversityCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateUniversityCommand request, CancellationToken cancellationToken)
    {
        if (await repository.IsSlugTakenAsync(request.Details.NormalizedSlug, null, cancellationToken))
        {
            return Result.Failure<Guid>(UniversityErrors.SlugTaken);
        }

        var university = request.Details.ToUniversity(currentUser.UserId, dateTimeProvider.UtcNow);

        repository.Insert(university);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return university.Id;
    }
}
