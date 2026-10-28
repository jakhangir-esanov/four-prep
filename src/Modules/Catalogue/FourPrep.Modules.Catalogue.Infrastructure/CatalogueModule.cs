namespace FourPrep.Modules.Catalogue.Infrastructure;

using Dapper;
using Microsoft.EntityFrameworkCore;
using FourPrep.Common.Domain.Enums;
using Microsoft.Extensions.Configuration;
using FourPrep.Common.Presentation.Endpoints;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using FourPrep.Modules.Catalogue.Domain.Programs;
using FourPrep.Modules.Catalogue.Infrastructure.Csv;
using FourPrep.Modules.Catalogue.Infrastructure.Json;
using FourPrep.Modules.Catalogue.Application.Abstractions.Csv;
using FourPrep.Modules.Catalogue.Domain.Destinations;
using FourPrep.Modules.Catalogue.Domain.Universities;
using FourPrep.Modules.Catalogue.Infrastructure.Database;
using FourPrep.Modules.Catalogue.Infrastructure.Programs;
using FourPrep.Modules.Catalogue.Application.Abstractions.Data;
using FourPrep.Modules.Catalogue.Infrastructure.Destinations;
using FourPrep.Modules.Catalogue.Infrastructure.Universities;
using FourPrep.Modules.Catalogue.Infrastructure.Authentication;
using FourPrep.Modules.Catalogue.Application.Abstractions.Authentication;

public static class CatalogueModule
{
    public static IServiceCollection AddCatalogueModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);

        services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CatalogueDbContext>((_, options) =>
            options
                .UseNpgsql(
                    configuration.GetConnectionString("Database"),
                    npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Catalogue))
                .UseSnakeCaseNamingConvention());

        services.AddScoped<ICurrentUser, CurrentUser>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CatalogueDbContext>());

        services.AddScoped<IDestinationRepository, DestinationRepository>();
        services.AddScoped<IUniversityRepository, UniversityRepository>();
        services.AddScoped<IProgramRepository, ProgramRepository>();
        services.AddSingleton<ICsvReader, CsvFileReader>();

        SqlMapper.AddTypeHandler(new JsonTypeHandler<Dictionary<string, string>>());
        SqlMapper.AddTypeHandler(new JsonTypeHandler<Dictionary<Exam, decimal>>());
    }
}
