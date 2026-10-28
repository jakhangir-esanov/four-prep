namespace FourPrep.Modules.Catalogue.Infrastructure.Programs;

using Microsoft.EntityFrameworkCore;
using FourPrep.Modules.Catalogue.Domain.Programs;
using FourPrep.Modules.Catalogue.Infrastructure.Database;

internal sealed class ProgramRepository(CatalogueDbContext dbContext) : IProgramRepository
{
    public async Task<Program?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Programs.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public void Insert(Program program)
    {
        dbContext.Programs.Add(program);
    }

    public void Remove(Program program)
    {
        dbContext.Programs.Remove(program);
    }
}
