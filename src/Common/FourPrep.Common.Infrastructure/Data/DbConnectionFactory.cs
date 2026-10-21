namespace FourPrep.Common.Infrastructure.Data;

using Npgsql;
using System.Data.Common;
using FourPrep.Common.Application.Data;

internal sealed class DbConnectionFactory(NpgsqlDataSource dataSource) : IDbConnectionFactory
{
    public async ValueTask<DbConnection> OpenConnectionAsync()
    {
        return await dataSource.OpenConnectionAsync();
    }
}
