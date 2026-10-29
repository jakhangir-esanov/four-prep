namespace FourPrep.Modules.Storage.Infrastructure.Database;

using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using FourPrep.Modules.Storage.Domain.StoredFiles;
using FourPrep.Modules.Storage.Infrastructure.StoredFiles;
using FourPrep.Modules.Storage.Application.Abstractions.Data;

public sealed class StorageDbContext(DbContextOptions<StorageDbContext> options)
    : DbContext(options), IUnitOfWork
{
    internal DbSet<StoredFile> StoredFiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Storage);

        modelBuilder.ApplyConfiguration(new StoredFileConfiguration());
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
