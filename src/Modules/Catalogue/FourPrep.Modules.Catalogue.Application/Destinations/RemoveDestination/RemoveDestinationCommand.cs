namespace FourPrep.Modules.Catalogue.Application.Destinations.RemoveDestination;

using FourPrep.Common.Domain;
using FourPrep.Common.Domain.Localization;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Catalogue.Domain.Destinations;
using FourPrep.Modules.Catalogue.Application.Abstractions.Data;

public sealed record RemoveDestinationCommand(string CountryCode) : ICommand;

internal sealed class RemoveDestinationCommandHandler(
    IUnitOfWork unitOfWork,
    IDestinationRepository repository) : ICommandHandler<RemoveDestinationCommand>
{
    public async Task<Result> Handle(RemoveDestinationCommand request, CancellationToken cancellationToken)
    {
        var destination = await repository.GetByCountryCodeAsync(
            Countries.Normalize(request.CountryCode),
            cancellationToken);
        if (destination is null)
        {
            return Result.Failure(DestinationErrors.NotFound);
        }

        repository.Remove(destination);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
