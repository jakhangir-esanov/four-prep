namespace FourPrep.Modules.Catalogue.Domain.Destinations;

public interface IDestinationRepository
{
    Task<Destination?> GetByCountryCodeAsync(string countryCode, CancellationToken cancellationToken = default);

    void Insert(Destination destination);

    void Remove(Destination destination);
}
