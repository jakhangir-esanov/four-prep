namespace FourPrep.Modules.Meeting.Infrastructure.StaffSchedules;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FourPrep.Modules.Meeting.Domain.StaffSchedules;
using FourPrep.Modules.Meeting.Domain.AvailabilityWindows;

internal sealed class StaffScheduleConfiguration : IEntityTypeConfiguration<StaffSchedule>
{
    private const string ScheduleForeignKey = "StaffScheduleId";
    private const string VersionProperty = "Version";

    public void Configure(EntityTypeBuilder<StaffSchedule> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.TimeZone).HasMaxLength(64).IsRequired();
        builder.Property(x => x.MeetingUrl).HasMaxLength(500);
        builder.Property(x => x.OfficeCountryCode).HasMaxLength(2);
        builder.Property(x => x.OfficeCity).HasMaxLength(100);
        builder.Property(x => x.OfficeAddress).HasMaxLength(300);
        builder.Property(x => x.CreatedOnUtc).IsRequired();
        builder.Property(x => x.UpdatedOnUtc).IsRequired();

        builder.OwnsMany(x => x.Windows, window =>
        {
            window.ToTable("availability_windows");
            window.WithOwner().HasForeignKey(ScheduleForeignKey);
            window.HasKey(
                ScheduleForeignKey,
                nameof(AvailabilityWindow.Day),
                nameof(AvailabilityWindow.Mode),
                nameof(AvailabilityWindow.StartTime));
        });

        builder.Navigation(x => x.Windows).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property<uint>(VersionProperty).IsRowVersion();

        builder.HasIndex(x => x.StaffUserId).IsUnique();
    }
}
