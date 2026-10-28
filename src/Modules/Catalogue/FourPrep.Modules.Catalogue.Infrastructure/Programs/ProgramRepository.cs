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

    public async Task<Program?> GetByNameAsync(Guid universityId, string name, CancellationToken cancellationToken = default)
    {
        return await dbContext.Programs.SingleOrDefaultAsync(
            x => x.UniversityId == universityId && x.Name == name,
            cancellationToken);
    }

    public async Task<bool> IsNameTakenAsync(
        Guid universityId,
        string name,
        Guid? exceptProgramId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Programs.AnyAsync(
            x => x.UniversityId == universityId && x.Name == name && (exceptProgramId == null || x.Id != exceptProgramId),
            cancellationToken);
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
