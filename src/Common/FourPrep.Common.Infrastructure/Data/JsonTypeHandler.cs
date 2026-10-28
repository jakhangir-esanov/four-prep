namespace FourPrep.Common.Infrastructure.Data;

using Dapper;
using System.Data;
using System.Text.Json;

public sealed class JsonTypeHandler<T> : SqlMapper.TypeHandler<T>
    where T : class, new()
{
    public override void SetValue(IDbDataParameter parameter, T? value)
    {
        parameter.DbType = DbType.String;
        parameter.Value = JsonSerializer.Serialize(value);
    }

    public override T Parse(object value) =>
        value is string json
            ? JsonSerializer.Deserialize<T>(json) ?? new T()
            : new T();
}
