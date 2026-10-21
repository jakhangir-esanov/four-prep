namespace FourPrep.Modules.Agreement.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using FourPrep.Common.Presentation.Endpoints;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using FourPrep.Modules.Agreement.Infrastructure.Database;
using FourPrep.Modules.Agreement.Application.Abstractions.Data;
using FourPrep.Modules.Agreement.Infrastructure.Authentication;
using FourPrep.Modules.Agreement.Application.Abstractions.Authentication;

public static class AgreementModule
{
    public static IServiceCollection AddAgreementModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);

        services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AgreementDbContext>((_, options) =>
            options
                .UseNpgsql(
                    configuration.GetConnectionString("Database"),
                    npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Agreement))
                .UseSnakeCaseNamingConvention());

        services.AddScoped<ICurrentUser, CurrentUser>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AgreementDbContext>());
    }
}
