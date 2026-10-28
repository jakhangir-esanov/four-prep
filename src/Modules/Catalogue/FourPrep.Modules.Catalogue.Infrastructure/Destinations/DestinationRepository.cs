namespace FourPrep.Modules.Catalogue.Infrastructure.Destinations;

using Microsoft.EntityFrameworkCore;
using FourPrep.Modules.Catalogue.Domain.Destinations;
using FourPrep.Modules.Catalogue.Infrastructure.Database;

internal sealed class DestinationRepository(CatalogueDbContext dbContext) : IDestinationRepository
{
    public async Task<Destination?> GetByCountryCodeAsync(string countryCode, CancellationToken cancellationToken = default)
    {
        return await dbContext.Destinations.SingleOrDefaultAsync(x => x.CountryCode == countryCode, cancellationToken);
    }

    public void Insert(Destination destination)
    {
        dbContext.Destinations.Add(destination);
    }

    public void Remove(Destination destination)
    {
        dbContext.Destinations.Remove(destination);
    }
}
