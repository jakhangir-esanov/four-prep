namespace FourPrep.Modules.Storage.Infrastructure.StoredFiles;

using Microsoft.EntityFrameworkCore;
using FourPrep.Modules.Storage.Domain.StoredFiles;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class StoredFileConfiguration : IEntityTypeConfiguration<StoredFile>
{
    public void Configure(EntityTypeBuilder<StoredFile> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UploadKey).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ObjectKey).HasMaxLength(100).IsRequired();
        builder.Property(x => x.FileName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Sha256).HasMaxLength(64);
        builder.Property(x => x.CreatedOnUtc).IsRequired();

        builder.HasIndex(x => x.UploadKey).IsUnique();
        builder.HasIndex(x => x.ObjectKey).IsUnique();
        builder.HasIndex(x => x.UploadedByUserId);
        builder.HasIndex(x => new { x.Status, x.CreatedOnUtc });
        builder.HasIndex(x => new { x.Status, x.ScanStatus });
    }
}
