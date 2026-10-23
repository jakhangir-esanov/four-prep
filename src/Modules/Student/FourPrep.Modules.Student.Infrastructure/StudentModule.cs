namespace FourPrep.Modules.Student.Infrastructure;

using Microsoft.EntityFrameworkCore;
using FourPrep.Modules.Student.Auth;
using Microsoft.Extensions.Configuration;
using FourPrep.Common.Presentation.Endpoints;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using FourPrep.Modules.Student.Domain.Students;
using FourPrep.Modules.Student.Infrastructure.Students;
using FourPrep.Modules.Student.Domain.StudentParents;
using FourPrep.Modules.Student.Infrastructure.Database;
using FourPrep.Modules.Student.Infrastructure.LinkCodes;
using FourPrep.Modules.Student.Application.Abstractions.Auth;
using FourPrep.Modules.Student.Application.Abstractions.Data;
using FourPrep.Modules.Student.Infrastructure.StudentParents;
using FourPrep.Modules.Student.Infrastructure.Authentication;
using FourPrep.Modules.Student.Application.Abstractions.LinkCodes;
using FourPrep.Modules.Student.Application.Abstractions.Authentication;

public static class StudentModule
{
    public static IServiceCollection AddStudentModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);

        services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<StudentDbContext>((_, options) =>
            options
                .UseNpgsql(
                    configuration.GetConnectionString("Database"),
                    npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Student))
                .UseSnakeCaseNamingConvention());

        services.AddScoped<ICurrentUser, CurrentUser>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<StudentDbContext>());

        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IStudentParentRepository, StudentParentRepository>();
        services.AddSingleton<ILinkCodeGenerator, LinkCodeGenerator>();
        services.AddScoped<IAuthService, AuthService>();
    }
}
