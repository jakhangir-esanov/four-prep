namespace FourPrep.Modules.Meeting.Infrastructure.Database;

using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using FourPrep.Modules.Meeting.Domain.Meetings;
using FourPrep.Modules.Meeting.Domain.StaffSchedules;
using FourPrep.Modules.Meeting.Infrastructure.Meetings;
using FourPrep.Modules.Meeting.Application.Abstractions.Data;
using FourPrep.Modules.Meeting.Infrastructure.StaffSchedules;

public sealed class MeetingDbContext(DbContextOptions<MeetingDbContext> options)
    : DbContext(options), IUnitOfWork
{
    internal DbSet<StaffSchedule> StaffSchedules { get; set; }

    internal DbSet<Meeting> Meetings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Meeting);

        modelBuilder.ApplyConfiguration(new StaffScheduleConfiguration());
        modelBuilder.ApplyConfiguration(new MeetingConfiguration());
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
