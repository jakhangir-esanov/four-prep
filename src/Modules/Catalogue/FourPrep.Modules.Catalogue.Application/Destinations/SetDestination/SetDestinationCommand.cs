namespace FourPrep.Modules.Catalogue.Application.Destinations.SetDestination;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Domain.Localization;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Catalogue.Domain.Destinations;
using FourPrep.Modules.Catalogue.Application.Abstractions.Data;

public sealed record SetDestinationCommand(string CountryCode, string VisaType) : ICommand;

internal sealed class SetDestinationCommandHandler(
    IUnitOfWork unitOfWork,
    IDestinationRepository repository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<SetDestinationCommand>
{
    public async Task<Result> Handle(SetDestinationCommand request, CancellationToken cancellationToken)
    {
        string countryCode = Countries.Normalize(request.CountryCode);
        string visaType = request.VisaType.Trim();

        var destination = await repository.GetByCountryCodeAsync(countryCode, cancellationToken);
        if (destination is null)
        {
            repository.Insert(Destination.Create(countryCode, visaType, dateTimeProvider.UtcNow));
        }
        else
        {
            destination.ChangeVisaType(visaType, dateTimeProvider.UtcNow);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
