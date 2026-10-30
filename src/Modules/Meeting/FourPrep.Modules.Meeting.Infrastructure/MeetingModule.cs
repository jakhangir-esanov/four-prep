namespace FourPrep.Modules.Meeting.Infrastructure;

using Microsoft.EntityFrameworkCore;
using FourPrep.Modules.Meeting.Auth;
using FourPrep.Modules.Meeting.Student;
using Microsoft.Extensions.Configuration;
using FourPrep.Common.Presentation.Endpoints;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using FourPrep.Modules.Meeting.Domain.Meetings;
using FourPrep.Modules.Meeting.Domain.StaffSchedules;
using FourPrep.Modules.Meeting.Infrastructure.Meetings;
using FourPrep.Modules.Meeting.Infrastructure.Database;
using FourPrep.Modules.Meeting.Infrastructure.StaffSchedules;
using FourPrep.Modules.Meeting.Application.Abstractions.Auth;
using FourPrep.Modules.Meeting.Application.Abstractions.Students;
using FourPrep.Modules.Meeting.Application.Abstractions.Data;
using FourPrep.Modules.Meeting.Infrastructure.Authentication;
using FourPrep.Modules.Meeting.Application.Abstractions.Authentication;

public static class MeetingModule
{
    public static IServiceCollection AddMeetingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);

        services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<MeetingDbContext>((_, options) =>
            options
                .UseNpgsql(
                    configuration.GetConnectionString("Database"),
                    npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Meeting))
                .UseSnakeCaseNamingConvention());

        services.AddScoped<ICurrentUser, CurrentUser>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<MeetingDbContext>());

        services.AddScoped<IStaffScheduleRepository, StaffScheduleRepository>();
        services.AddScoped<IMeetingRepository, MeetingRepository>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IStudentService, StudentService>();
    }
}
