namespace FourPrep.Modules.Student.Infrastructure.StudentParents;

using Microsoft.EntityFrameworkCore;
using FourPrep.Modules.Student.Domain.Students;
using FourPrep.Modules.Student.Domain.StudentParents;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class StudentParentConfiguration : IEntityTypeConfiguration<StudentParent>
{
    public void Configure(EntityTypeBuilder<StudentParent> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ParentUserName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.LinkedOnUtc).IsRequired();

        builder.HasOne<Student>()
            .WithMany()
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.StudentId, x.ParentUserId }).IsUnique();
        builder.HasIndex(x => x.ParentUserId);
    }
}
