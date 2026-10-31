namespace FourPrep.Modules.Admission.Infrastructure.Database;

using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using FourPrep.Modules.Admission.Application.Abstractions.Data;
using FourPrep.Modules.Admission.Domain.AcademyApplications;
using FourPrep.Modules.Admission.Infrastructure.AcademyApplications;

public sealed class AdmissionDbContext(DbContextOptions<AdmissionDbContext> options)
    : DbContext(options), IUnitOfWork
{
    internal DbSet<AcademyApplication> AcademyApplications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Admission);

        modelBuilder.ApplyConfiguration(new AcademyApplicationConfiguration());
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
