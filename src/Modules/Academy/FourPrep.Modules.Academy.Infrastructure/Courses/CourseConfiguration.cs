namespace FourPrep.Modules.Academy.Infrastructure.Courses;

using Microsoft.EntityFrameworkCore;
using FourPrep.Common.Infrastructure.Data;
using FourPrep.Modules.Academy.Domain.Courses;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Slug).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Name).HasJsonbConversion().IsRequired();
        builder.Property(x => x.Description).HasJsonbConversion().IsRequired();
        builder.Property(x => x.CreatedOnUtc).IsRequired();
        builder.Property(x => x.UpdatedOnUtc).IsRequired();

        builder.HasIndex(x => x.Slug).IsUnique();
    }
}
