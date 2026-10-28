namespace FourPrep.Modules.Academy.Infrastructure;

using Microsoft.EntityFrameworkCore;
using FourPrep.Modules.Academy.Auth;
using Microsoft.Extensions.Configuration;
using FourPrep.Common.Presentation.Endpoints;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using FourPrep.Modules.Academy.Domain.Courses;
using FourPrep.Modules.Academy.Domain.ClassGroups;
using FourPrep.Modules.Academy.Infrastructure.Courses;
using FourPrep.Modules.Academy.Infrastructure.Database;
using FourPrep.Modules.Academy.Application.Abstractions.Auth;
using FourPrep.Modules.Academy.Infrastructure.ClassGroups;
using FourPrep.Modules.Academy.Application.Abstractions.Data;
using FourPrep.Modules.Academy.Infrastructure.Authentication;
using FourPrep.Modules.Academy.Application.Abstractions.Authentication;

public static class AcademyModule
{
    public static IServiceCollection AddAcademyModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);

        services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AcademyDbContext>((_, options) =>
            options
                .UseNpgsql(
                    configuration.GetConnectionString("Database"),
                    npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Academy))
                .UseSnakeCaseNamingConvention());

        services.AddScoped<ICurrentUser, CurrentUser>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AcademyDbContext>());

        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<IClassGroupRepository, ClassGroupRepository>();
        services.AddScoped<IAuthService, AuthService>();
    }
}
