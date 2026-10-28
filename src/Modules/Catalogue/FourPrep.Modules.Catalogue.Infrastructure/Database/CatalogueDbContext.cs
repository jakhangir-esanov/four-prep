namespace FourPrep.Modules.Catalogue.Infrastructure.Database;

using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using FourPrep.Modules.Catalogue.Domain.Programs;
using FourPrep.Modules.Catalogue.Domain.Destinations;
using FourPrep.Modules.Catalogue.Domain.Universities;
using FourPrep.Modules.Catalogue.Infrastructure.Programs;
using FourPrep.Modules.Catalogue.Application.Abstractions.Data;
using FourPrep.Modules.Catalogue.Infrastructure.Destinations;
using FourPrep.Modules.Catalogue.Infrastructure.Universities;

public sealed class CatalogueDbContext(DbContextOptions<CatalogueDbContext> options)
    : DbContext(options), IUnitOfWork
{
    internal DbSet<Destination> Destinations { get; set; }

    internal DbSet<University> Universities { get; set; }

    internal DbSet<Program> Programs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Catalogue);

        modelBuilder.ApplyConfiguration(new DestinationConfiguration());
        modelBuilder.ApplyConfiguration(new UniversityConfiguration());
        modelBuilder.ApplyConfiguration(new ProgramConfiguration());
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
