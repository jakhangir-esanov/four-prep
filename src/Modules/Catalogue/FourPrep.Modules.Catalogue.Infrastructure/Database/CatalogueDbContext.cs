namespace FourPrep.Modules.Catalogue.Infrastructure.Database;

using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using FourPrep.Modules.Catalogue.Application.Abstractions.Data;

public sealed class CatalogueDbContext(DbContextOptions<CatalogueDbContext> options)
    : DbContext(options), IUnitOfWork
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Catalogue);
    }

    public async Task<DbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction is not null)
        {
            await Database.CurrentTransaction.DisposeAsync();
        }

        return (await Database.BeginTransactionAsync(cancellationToken)).GetDbTransaction();
    }
}
