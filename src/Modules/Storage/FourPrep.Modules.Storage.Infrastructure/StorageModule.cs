namespace FourPrep.Modules.Storage.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using FourPrep.Common.Presentation.Endpoints;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using FourPrep.Modules.Storage.Infrastructure.Database;
using FourPrep.Modules.Storage.Application.Abstractions.Data;
using FourPrep.Modules.Storage.Infrastructure.Authentication;
using FourPrep.Modules.Storage.Application.Abstractions.Authentication;

public static class StorageModule
{
    public static IServiceCollection AddStorageModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);

        services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<StorageDbContext>((_, options) =>
            options
                .UseNpgsql(
                    configuration.GetConnectionString("Database"),
                    npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Storage))
                .UseSnakeCaseNamingConvention());

        services.AddScoped<ICurrentUser, CurrentUser>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<StorageDbContext>());
    }
}
