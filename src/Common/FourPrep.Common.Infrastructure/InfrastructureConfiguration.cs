namespace FourPrep.Common.Infrastructure;

using Npgsql;
using Dapper;
using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Infrastructure.Data;
using FourPrep.Common.Infrastructure.Clock;
using FourPrep.Common.Infrastructure.Files;
using FourPrep.Common.Application.Files;
using Microsoft.Extensions.Configuration;
using FourPrep.Common.Application.Currencies;
using FourPrep.Common.Infrastructure.Currencies;
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

        services.AddMemoryCache();
        services.AddHttpClient<CbuExchangeRateClient>(client =>
            client.BaseAddress = new Uri(CbuExchangeRateClient.BaseUrl));
        services.TryAddScoped<ICurrencyConverter, CurrencyConverter>();

        services.AddHttpContextAccessor();

        SqlMapper.AddTypeHandler(new GenericArrayHandler<string>());
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
        SqlMapper.AddTypeHandler(new TimeOnlyTypeHandler());

        return services;
    }

    public static IServiceCollection AddFileLinks(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FileLinkOptions>(configuration.GetSection(FileLinkOptions.SectionName));
        services.TryAddSingleton<IFileUrlSigner, HmacFileUrlSigner>();

        return services;
    }
}
