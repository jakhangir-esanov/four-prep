namespace FourPrep.Modules.Academy.Infrastructure.ClassGroups;

using Microsoft.EntityFrameworkCore;
using FourPrep.Common.Infrastructure.Data;
using FourPrep.Modules.Academy.Domain.Courses;
using FourPrep.Modules.Academy.Domain.ClassGroups;
using FourPrep.Modules.Academy.Domain.ScheduleSlots;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class ClassGroupConfiguration : IEntityTypeConfiguration<ClassGroup>
{
    private const string ClassGroupForeignKey = "ClassGroupId";

    public void Configure(EntityTypeBuilder<ClassGroup> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CountryCode).HasMaxLength(2);
        builder.Property(x => x.City).HasMaxLength(100);
        builder.Property(x => x.Address).HasMaxLength(300);
        builder.Property(x => x.EntryLevel).HasJsonbConversion().IsRequired();
        builder.Property(x => x.TimeZone).HasMaxLength(64).IsRequired();
        builder.Property(x => x.TeacherName).HasMaxLength(200);
        builder.Property(x => x.CreatedOnUtc).IsRequired();
        builder.Property(x => x.UpdatedOnUtc).IsRequired();

        builder.HasOne<Course>()
            .WithMany()
            .HasForeignKey(x => x.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsMany(x => x.Schedule, slot =>
        {
            slot.ToTable("schedule_slots");
            slot.WithOwner().HasForeignKey(ClassGroupForeignKey);
            slot.HasKey(ClassGroupForeignKey, nameof(ScheduleSlot.Day), nameof(ScheduleSlot.StartTime));
            slot.Ignore(x => x.EndTime);
        });

        builder.Navigation(x => x.Schedule).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => new { x.Status, x.StartsOn });
        builder.HasIndex(x => x.TeacherUserId);
    }
}
