namespace FourPrep.Api.Extensions;

using Microsoft.EntityFrameworkCore;
using FourPrep.Modules.AI.Infrastructure.Database;
using FourPrep.Modules.Auth.Infrastructure.Database;
using FourPrep.Modules.Chat.Infrastructure.Database;
using FourPrep.Modules.Audit.Infrastructure.Database;
using FourPrep.Modules.Academy.Infrastructure.Database;
using FourPrep.Modules.Content.Infrastructure.Database;
using FourPrep.Modules.Meeting.Infrastructure.Database;
using FourPrep.Modules.Payment.Infrastructure.Database;
using FourPrep.Modules.Storage.Infrastructure.Database;
using FourPrep.Modules.Student.Infrastructure.Database;
using FourPrep.Modules.Admission.Infrastructure.Database;
using FourPrep.Modules.Agreement.Infrastructure.Database;
using FourPrep.Modules.Catalogue.Infrastructure.Database;
using FourPrep.Modules.Notification.Infrastructure.Database;

internal sealed class DatabaseOptions
{
    internal const string SectionName = "Database";

    public bool ApplyMigrationsOnStartup { get; set; }
}

internal static class MigrationExtensions
{
    internal static Task InitializeDatabaseAsync(this WebApplication app)
    {
        DatabaseOptions options = app.Configuration
            .GetSection(DatabaseOptions.SectionName)
            .Get<DatabaseOptions>() ?? new DatabaseOptions();

        if (options.ApplyMigrationsOnStartup)
        {
            app.ApplyMigrations();
        }

        return Task.CompletedTask;
    }

    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        ApplyMigration<AuthDbContext>(scope);
        ApplyMigration<StorageDbContext>(scope);
        ApplyMigration<NotificationDbContext>(scope);
        ApplyMigration<ChatDbContext>(scope);
        ApplyMigration<PaymentDbContext>(scope);
        ApplyMigration<StudentDbContext>(scope);
        ApplyMigration<ContentDbContext>(scope);
        ApplyMigration<AIDbContext>(scope);
        ApplyMigration<AcademyDbContext>(scope);
        ApplyMigration<AgreementDbContext>(scope);
        ApplyMigration<AdmissionDbContext>(scope);
        ApplyMigration<AuditDbContext>(scope);
        ApplyMigration<CatalogueDbContext>(scope);
        ApplyMigration<MeetingDbContext>(scope);
    }

    private static void ApplyMigration<TDbContext>(IServiceScope scope)
        where TDbContext : DbContext
    {
        using TDbContext context = scope.ServiceProvider.GetRequiredService<TDbContext>();

        context.Database.Migrate();
    }
}
