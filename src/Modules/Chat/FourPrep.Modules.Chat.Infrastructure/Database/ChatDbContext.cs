namespace FourPrep.Modules.Chat.Infrastructure.Database;

using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using FourPrep.Modules.Chat.Domain.ChatThreads;
using FourPrep.Modules.Chat.Domain.ChatMessages;
using FourPrep.Modules.Chat.Domain.ChatReadMarks;
using FourPrep.Modules.Chat.Infrastructure.ChatThreads;
using FourPrep.Modules.Chat.Application.Abstractions.Data;
using FourPrep.Modules.Chat.Infrastructure.ChatMessages;
using FourPrep.Modules.Chat.Infrastructure.ChatReadMarks;

public sealed class ChatDbContext(DbContextOptions<ChatDbContext> options)
    : DbContext(options), IUnitOfWork
{
    internal DbSet<ChatThread> ChatThreads { get; set; }
    internal DbSet<ChatMessage> ChatMessages { get; set; }
    internal DbSet<ChatReadMark> ChatReadMarks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Chat);

        modelBuilder.ApplyConfiguration(new ChatThreadConfiguration());
        modelBuilder.ApplyConfiguration(new ChatMessageConfiguration());
        modelBuilder.ApplyConfiguration(new ChatReadMarkConfiguration());
    }

    public async Task<DbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction is not null)
        {
            await Database.CurrentTransaction.DisposeAsync();
        }

        return (await Database.BeginTransactionAsync(cancellationToken)).GetDbTransaction();
    }
}
