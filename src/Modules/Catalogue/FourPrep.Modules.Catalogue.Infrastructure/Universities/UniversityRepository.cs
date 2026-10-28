namespace FourPrep.Modules.Catalogue.Infrastructure.Universities;

using Microsoft.EntityFrameworkCore;
using FourPrep.Modules.Catalogue.Domain.Universities;
using FourPrep.Modules.Catalogue.Infrastructure.Database;

internal sealed class UniversityRepository(CatalogueDbContext dbContext) : IUniversityRepository
{
    public async Task<University?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Universities.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<University?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await dbContext.Universities.SingleOrDefaultAsync(x => x.Slug == slug, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Universities.AnyAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> IsSlugTakenAsync(
        string slug,
        Guid? exceptUniversityId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Universities.AnyAsync(
            x => x.Slug == slug && (exceptUniversityId == null || x.Id != exceptUniversityId),
            cancellationToken);
    }

    public void Insert(University university)
    {
        dbContext.Universities.Add(university);
    }

    public void Remove(University university)
    {
        dbContext.Universities.Remove(university);
    }
}
