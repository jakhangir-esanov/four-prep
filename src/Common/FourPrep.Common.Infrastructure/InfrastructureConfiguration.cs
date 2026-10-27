namespace FourPrep.Common.Infrastructure;

using Npgsql;
using Dapper;
using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Infrastructure.Data;
using FourPrep.Common.Infrastructure.Clock;
using Microsoft.Extensions.DependencyInjection;
using FourPrep.Common.Infrastructure.Authentication;
using Microsoft.Extensions.DependencyInjection.Extensions;

public static class InfrastructureConfiguration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string databaseConnectionString)
    {
        services.AddAuthenticationInternal();

        services.TryAddSingleton<IDateTimeProvider, DateTimeProvider>();

        NpgsqlDataSource npgsqlDataSource = new NpgsqlDataSourceBuilder(databaseConnectionString).Build();
        services.TryAddSingleton(npgsqlDataSource);

        services.AddHealthChecks().AddNpgSql(databaseConnectionString);

        services.TryAddScoped<IDbConnectionFactory, DbConnectionFactory>();
        services.TryAddSingleton<ITimeZoneContext, TimeZoneContext>();

        services.AddHttpContextAccessor();

        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());

        return services;
    }
}
