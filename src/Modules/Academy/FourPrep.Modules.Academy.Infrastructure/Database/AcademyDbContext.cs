namespace FourPrep.Modules.Academy.Infrastructure.Database;

using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using FourPrep.Modules.Academy.Domain.Courses;
using FourPrep.Modules.Academy.Domain.ClassGroups;
using FourPrep.Modules.Academy.Infrastructure.Courses;
using FourPrep.Modules.Academy.Infrastructure.ClassGroups;
using FourPrep.Modules.Academy.Application.Abstractions.Data;

public sealed class AcademyDbContext(DbContextOptions<AcademyDbContext> options)
    : DbContext(options), IUnitOfWork
{
    internal DbSet<Course> Courses { get; set; }

    internal DbSet<ClassGroup> ClassGroups { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Academy);

        modelBuilder.ApplyConfiguration(new CourseConfiguration());
        modelBuilder.ApplyConfiguration(new ClassGroupConfiguration());
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
