namespace FourPrep.Modules.Content.Infrastructure.NewsPosts;

using Microsoft.EntityFrameworkCore;
using FourPrep.Common.Infrastructure.Data;
using FourPrep.Modules.Content.Domain.NewsPosts;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class NewsPostConfiguration : IEntityTypeConfiguration<NewsPost>
{
    public void Configure(EntityTypeBuilder<NewsPost> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Slug).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Title).HasJsonbConversion().IsRequired();
        builder.Property(x => x.Body).HasJsonbConversion().IsRequired();
        builder.Property(x => x.Tag).HasMaxLength(50);
        builder.Property(x => x.CoverUrl).HasMaxLength(500);
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.CreatedOnUtc).IsRequired();
        builder.Property(x => x.UpdatedOnUtc).IsRequired();

        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => x.CoverFileId).IsUnique();
        builder.HasIndex(x => new { x.Status, x.PublishedOnUtc });
        builder.HasIndex(x => x.Tag);
    }
}
