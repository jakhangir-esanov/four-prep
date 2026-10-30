namespace FourPrep.Modules.Content.Infrastructure.Database;

using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using FourPrep.Modules.Content.Domain.NewsPosts;
using FourPrep.Modules.Content.Infrastructure.NewsPosts;
using FourPrep.Modules.Content.Application.Abstractions.Data;

public sealed class ContentDbContext(DbContextOptions<ContentDbContext> options)
    : DbContext(options), IUnitOfWork
{
    internal DbSet<NewsPost> NewsPosts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Content);

        modelBuilder.ApplyConfiguration(new NewsPostConfiguration());
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
