namespace FourPrep.Common.Application.Data;

using System.Data.Common;

public interface IDbConnectionFactory
{
    ValueTask<DbConnection> OpenConnectionAsync();
}
