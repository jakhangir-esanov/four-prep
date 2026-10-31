namespace FourPrep.Modules.Chat.Infrastructure.ChatThreads;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FourPrep.Modules.Chat.Domain.ChatThreads;

internal sealed class ChatThreadConfiguration : IEntityTypeConfiguration<ChatThread>
{
    public void Configure(EntityTypeBuilder<ChatThread> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.StudentName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.CreatedOnUtc).IsRequired();
        builder.Property(x => x.UpdatedOnUtc).IsRequired();

        builder.HasIndex(x => new { x.StudentUserId, x.Kind }).IsUnique();
    }
}
