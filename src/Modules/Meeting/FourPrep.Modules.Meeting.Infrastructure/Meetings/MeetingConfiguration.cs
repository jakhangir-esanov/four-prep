namespace FourPrep.Modules.Meeting.Infrastructure.Meetings;

using Microsoft.EntityFrameworkCore;
using FourPrep.Modules.Meeting.Domain.Meetings;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class MeetingConfiguration : IEntityTypeConfiguration<Meeting>
{
    private const string ScheduledOnly = "status = 1";

    public void Configure(EntityTypeBuilder<Meeting> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.StudentName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.StudentPhone).HasMaxLength(20);
        builder.Property(x => x.StaffName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.MeetingUrl).HasMaxLength(500);
        builder.Property(x => x.OfficeCountryCode).HasMaxLength(2);
        builder.Property(x => x.OfficeCity).HasMaxLength(100);
        builder.Property(x => x.OfficeAddress).HasMaxLength(300);
        builder.Property(x => x.StaffNotes).HasMaxLength(4000);
        builder.Property(x => x.CancellationReason).HasMaxLength(500);
        builder.Property(x => x.CreatedOnUtc).IsRequired();
        builder.Property(x => x.UpdatedOnUtc).IsRequired();

        builder.Ignore(x => x.EndsOnUtc);

        builder.HasIndex(x => new { x.StaffUserId, x.StartsOnUtc }).IsUnique().HasFilter(ScheduledOnly);
        builder.HasIndex(x => new { x.StudentUserId, x.Kind }).IsUnique().HasFilter(ScheduledOnly);
        builder.HasIndex(x => new { x.StudentUserId, x.StartsOnUtc });
        builder.HasIndex(x => new { x.StaffUserId, x.Status, x.StartsOnUtc });
        builder.HasIndex(x => x.StartsOnUtc);
        builder.HasIndex(x => new { x.BookedByUserId, x.Status });
    }
}
