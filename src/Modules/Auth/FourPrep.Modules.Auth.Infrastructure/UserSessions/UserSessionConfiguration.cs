namespace FourPrep.Modules.Auth.Infrastructure.UserSessions;

using FourPrep.Modules.Auth.Domain.UserSessions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

internal sealed class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.IdentitySessionId).HasMaxLength(64);
        builder.Property(x => x.AccessTokenHash)
            .HasMaxLength(512)
            .IsRequired();
        builder.Property(x => x.ExpiresOnUtc).IsRequired();
        builder.Property(x => x.CreatedOnUtc).IsRequired();

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.IdentitySessionId);
        builder.HasIndex(x => x.AccessTokenHash);
        builder.HasIndex(x => x.ExpiresOnUtc);
    }
}
