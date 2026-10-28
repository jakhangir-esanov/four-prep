namespace FourPrep.Modules.Audit.Infrastructure.Database;

using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using FourPrep.Modules.Audit.Application.Abstractions.Data;

public sealed class AuditDbContext(DbContextOptions<AuditDbContext> options)
    : DbContext(options), IUnitOfWork
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Audit);
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
