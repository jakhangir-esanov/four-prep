namespace FourPrep.Modules.Student.Infrastructure.Database;

using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using FourPrep.Modules.Student.Domain.Students;
using FourPrep.Modules.Student.Domain.StudentParents;
using FourPrep.Modules.Student.Infrastructure.Students;
using FourPrep.Modules.Student.Application.Abstractions.Data;
using FourPrep.Modules.Student.Infrastructure.StudentParents;

public sealed class StudentDbContext(DbContextOptions<StudentDbContext> options)
    : DbContext(options), IUnitOfWork
{
    internal DbSet<Student> Students { get; set; }

    internal DbSet<StudentParent> StudentParents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Student);

        modelBuilder.ApplyConfiguration(new StudentConfiguration());
        modelBuilder.ApplyConfiguration(new StudentParentConfiguration());
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
