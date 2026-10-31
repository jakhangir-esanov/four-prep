namespace FourPrep.Modules.Chat.Infrastructure;

using Microsoft.EntityFrameworkCore;
using FourPrep.Modules.Chat.Student;
using Microsoft.Extensions.Configuration;
using FourPrep.Common.Presentation.Endpoints;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using FourPrep.Modules.Chat.Domain.ChatThreads;
using FourPrep.Modules.Chat.Domain.ChatMessages;
using FourPrep.Modules.Chat.Domain.ChatReadMarks;
using FourPrep.Modules.Chat.Infrastructure.Database;
using FourPrep.Modules.Chat.Infrastructure.ChatThreads;
using FourPrep.Modules.Chat.Application.Abstractions.Data;
using FourPrep.Modules.Chat.Infrastructure.ChatMessages;
using FourPrep.Modules.Chat.Infrastructure.ChatReadMarks;
using FourPrep.Modules.Chat.Infrastructure.Authentication;
using FourPrep.Modules.Chat.Application.Abstractions.Students;
using FourPrep.Modules.Chat.Application.Abstractions.Authentication;

public static class ChatModule
{
    public static IServiceCollection AddChatModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);

        services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ChatDbContext>((_, options) =>
            options
                .UseNpgsql(
                    configuration.GetConnectionString("Database"),
                    npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Chat))
                .UseSnakeCaseNamingConvention());

        services.AddScoped<ICurrentUser, CurrentUser>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ChatDbContext>());

        services.AddScoped<IChatThreadRepository, ChatThreadRepository>();
        services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
        services.AddScoped<IChatReadMarkRepository, ChatReadMarkRepository>();
        services.AddScoped<IStudentService, StudentService>();
    }
}
