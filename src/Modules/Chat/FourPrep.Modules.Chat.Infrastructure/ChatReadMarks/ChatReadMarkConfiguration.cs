namespace FourPrep.Modules.Chat.Infrastructure.ChatReadMarks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FourPrep.Modules.Chat.Domain.ChatThreads;
using FourPrep.Modules.Chat.Domain.ChatReadMarks;

internal sealed class ChatReadMarkConfiguration : IEntityTypeConfiguration<ChatReadMark>
{
    public void Configure(EntityTypeBuilder<ChatReadMark> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.LastReadOnUtc).IsRequired();

        builder.HasOne<ChatThread>()
            .WithMany()
            .HasForeignKey(x => x.ThreadId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.ThreadId, x.UserId }).IsUnique();
    }
}
