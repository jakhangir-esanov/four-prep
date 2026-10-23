namespace FourPrep.Modules.Auth.Infrastructure;

using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Modules.Auth.Domain.UserSessions;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using FourPrep.Common.Infrastructure.Configuration;
using FourPrep.Modules.Auth.Student;
using FourPrep.Modules.Auth.Infrastructure.Database;
using FourPrep.Modules.Auth.Infrastructure.Identity;
using FourPrep.Modules.Auth.Infrastructure.UserSessions;
using FourPrep.Modules.Auth.Infrastructure.Authentication;
using FourPrep.Modules.Auth.Application.Abstractions.Data;
using FourPrep.Modules.Auth.Application.Abstractions.Identity;
using FourPrep.Modules.Auth.Application.Abstractions.Students;
using FourPrep.Modules.Auth.Application.Abstractions.Authentication;

public static class AuthModule
{
    public static IServiceCollection AddAuthModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);
        services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AuthDbContext>((sp, options) =>
            options
                .UseNpgsql(
                    configuration.GetConnectionString("Database"),
                    npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Auth))
                .UseSnakeCaseNamingConvention());

        services.Configure<KeycloakOptions>(configuration.GetSection("Identity:Keycloak"));
        services.AddHttpClient<IIdentityProviderClient, KeycloakAdminClient>((sp, client) =>
        {
            var keycloakOptions = sp.GetRequiredService<IOptions<KeycloakOptions>>().Value;
            client.BaseAddress = new Uri($"{keycloakOptions.BaseUrl.TrimEnd('/')}/");
        });

        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IUserSessionRepository, UserSessionRepository>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AuthDbContext>());

        services.AddHangfire(hangfireConfiguration =>
            hangfireConfiguration.UsePostgreSqlStorage(options =>
                options.UseNpgsqlConnection(configuration.GetConnectionStringOrThrow("Database"))));

        services.AddHangfireServer();
    }
}
