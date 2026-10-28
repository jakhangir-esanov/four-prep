namespace FourPrep.Modules.Catalogue.Infrastructure.Programs;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FourPrep.Modules.Catalogue.Domain.Programs;
using FourPrep.Modules.Catalogue.Infrastructure.Json;
using FourPrep.Modules.Catalogue.Domain.Universities;

internal sealed class ProgramConfiguration : IEntityTypeConfiguration<Program>
{
    private const string ProgramForeignKey = "ProgramId";

    public void Configure(EntityTypeBuilder<Program> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Field).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Language).HasMaxLength(2).IsRequired();
        builder.Property(x => x.SourceUrl).HasMaxLength(500).IsRequired();
        builder.Property(x => x.CreatedOnUtc).IsRequired();
        builder.Property(x => x.UpdatedOnUtc).IsRequired();

        builder.HasOne<University>()
            .WithMany()
            .HasForeignKey(x => x.UniversityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsMany(x => x.Requirements, requirement =>
        {
            requirement.ToTable("admission_requirements");
            requirement.WithOwner().HasForeignKey(ProgramForeignKey);
            requirement.HasKey(x => x.Id);
            requirement.Property(x => x.Id).ValueGeneratedNever();
            requirement.Property(x => x.EnglishMinimums).HasJsonbConversion().IsRequired();
            requirement.Property(x => x.MinimumGpa).HasPrecision(5, 2);
            requirement.Property(x => x.GpaScale).HasPrecision(5, 2);
            requirement.Property(x => x.SourceUrl).HasMaxLength(500).IsRequired();
        });

        builder.OwnsMany(x => x.Deadlines, deadline =>
        {
            deadline.ToTable("deadlines");
            deadline.WithOwner().HasForeignKey(ProgramForeignKey);
            deadline.HasKey(x => x.Id);
            deadline.Property(x => x.Id).ValueGeneratedNever();
            deadline.Property(x => x.Round).HasMaxLength(100).IsRequired();
            deadline.Property(x => x.SourceUrl).HasMaxLength(500).IsRequired();
        });

        builder.Navigation(x => x.Requirements).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.Deadlines).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.UniversityId);
        builder.HasIndex(x => x.Field);
        builder.HasIndex(x => x.DegreeLevel);
    }
}
