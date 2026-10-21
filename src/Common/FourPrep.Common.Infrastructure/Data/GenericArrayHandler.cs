namespace FourPrep.Common.Infrastructure.Data;

using Dapper;
using System.Data;

internal sealed class GenericArrayHandler<T> : SqlMapper.TypeHandler<T[]>
{
    public override void SetValue(IDbDataParameter parameter, T[]? value)
    {
        parameter.Value = value;
    }

    public override T[]? Parse(object value)
    {
        return value switch
        {
            T[] typed => typed,
            int[] numbers when typeof(T).IsEnum => numbers.Select(number => (T)Enum.ToObject(typeof(T), number)).ToArray(),
            _ => null
        };
    }
}
