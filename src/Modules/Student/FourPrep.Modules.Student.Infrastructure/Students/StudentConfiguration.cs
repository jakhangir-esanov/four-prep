namespace FourPrep.Modules.Student.Infrastructure.Students;

using Microsoft.EntityFrameworkCore;
using FourPrep.Modules.Student.Domain.Students;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.FirstName).HasMaxLength(100);
        builder.Property(x => x.LastName).HasMaxLength(100);
        builder.Property(x => x.School).HasMaxLength(200);
        builder.Property(x => x.Country).HasMaxLength(2);
        builder.Property(x => x.Region).HasMaxLength(100);
        builder.Property(x => x.LinkCode).HasMaxLength(16).IsRequired();
        builder.Property(x => x.CreatedOnUtc).IsRequired();
        builder.Property(x => x.UpdatedOnUtc).IsRequired();

        builder.OwnsMany(x => x.ExamGoals, examGoal =>
        {
            examGoal.ToTable("exam_goals");
            examGoal.WithOwner().HasForeignKey("StudentId");
            examGoal.HasKey("StudentId", nameof(Domain.ExamGoals.ExamGoal.Exam));
            examGoal.Property(x => x.TargetScore).HasPrecision(6, 1);
        });

        builder.Navigation(x => x.ExamGoals).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.UserId).IsUnique();
        builder.HasIndex(x => x.LinkCode).IsUnique();
        builder.HasIndex(x => x.MentorUserId);
        builder.HasIndex(x => x.Country);
    }
}
