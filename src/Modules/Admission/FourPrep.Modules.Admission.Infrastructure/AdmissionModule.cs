namespace FourPrep.Modules.Admission.Infrastructure;

using Dapper;
using Microsoft.EntityFrameworkCore;
using FourPrep.Modules.Admission.Student;
using Microsoft.Extensions.Configuration;
using FourPrep.Common.Infrastructure.Data;
using FourPrep.Common.Presentation.Endpoints;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using FourPrep.Modules.Admission.Infrastructure.Database;
using FourPrep.Modules.Admission.Domain.AcademyApplications;
using FourPrep.Modules.Admission.Application.Abstractions.Data;
using FourPrep.Modules.Admission.Infrastructure.Authentication;
using FourPrep.Modules.Admission.Application.Abstractions.Students;
using FourPrep.Modules.Admission.Infrastructure.AcademyApplications;
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

        services.AddScoped<IAcademyApplicationRepository, AcademyApplicationRepository>();
        services.AddScoped<IStudentService, StudentService>();

        SqlMapper.AddTypeHandler(new JsonTypeHandler<List<ApplicationInterest>>());
        SqlMapper.AddTypeHandler(new JsonTypeHandler<List<ContactChannel>>());
    }
}
