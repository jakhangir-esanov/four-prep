namespace FourPrep.Modules.Catalogue.Infrastructure.Universities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FourPrep.Modules.Catalogue.Infrastructure.Json;
using FourPrep.Modules.Catalogue.Domain.Universities;

internal sealed class UniversityConfiguration : IEntityTypeConfiguration<University>
{
    private const string UniversityForeignKey = "UniversityId";

    public void Configure(EntityTypeBuilder<University> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Slug).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.CountryCode).HasMaxLength(2).IsRequired();
        builder.Property(x => x.City).HasMaxLength(100);
        builder.Property(x => x.Website).HasMaxLength(500);
        builder.Property(x => x.RankingSource).HasMaxLength(200);
        builder.Property(x => x.AcceptanceRate).HasPrecision(5, 2);
        builder.Property(x => x.Description).HasJsonbConversion().IsRequired();
        builder.Property(x => x.SourceUrl).HasMaxLength(500).IsRequired();
        builder.Property(x => x.CreatedOnUtc).IsRequired();
        builder.Property(x => x.UpdatedOnUtc).IsRequired();

        builder.OwnsMany(x => x.Costs, cost =>
        {
            cost.ToTable("costs");
            cost.WithOwner().HasForeignKey(UniversityForeignKey);
            cost.HasKey(x => x.Id);
            cost.Property(x => x.Id).ValueGeneratedNever();
            cost.Property(x => x.Currency).HasMaxLength(3).IsRequired();
            cost.Property(x => x.SourceUrl).HasMaxLength(500).IsRequired();
        });

        builder.OwnsMany(x => x.Scholarships, scholarship =>
        {
            scholarship.ToTable("scholarships");
            scholarship.WithOwner().HasForeignKey(UniversityForeignKey);
            scholarship.HasKey(x => x.Id);
            scholarship.Property(x => x.Id).ValueGeneratedNever();
            scholarship.Property(x => x.Name).HasMaxLength(200).IsRequired();
            scholarship.Property(x => x.Amount).HasJsonbConversion().IsRequired();
            scholarship.Property(x => x.Eligibility).HasJsonbConversion().IsRequired();
            scholarship.Property(x => x.SourceUrl).HasMaxLength(500).IsRequired();
        });

        builder.Navigation(x => x.Costs).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.Scholarships).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => x.CountryCode);
    }
}
