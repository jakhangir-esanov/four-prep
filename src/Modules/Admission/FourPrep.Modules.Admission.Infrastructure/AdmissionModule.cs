namespace FourPrep.Modules.Admission.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using FourPrep.Common.Presentation.Endpoints;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using FourPrep.Modules.Admission.Infrastructure.Database;
using FourPrep.Modules.Admission.Application.Abstractions.Data;
using FourPrep.Modules.Admission.Infrastructure.Authentication;
using FourPrep.Modules.Admission.Application.Abstractions.Authentication;

public static class AdmissionModule
{
    public static IServiceCollection AddAdmissionModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);

        services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AdmissionDbContext>((_, options) =>
            options
                .UseNpgsql(
                    configuration.GetConnectionString("Database"),
                    npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Admission))
                .UseSnakeCaseNamingConvention());

        services.AddScoped<ICurrentUser, CurrentUser>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AdmissionDbContext>());
    }
}
