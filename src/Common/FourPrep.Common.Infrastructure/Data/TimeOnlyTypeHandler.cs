namespace FourPrep.Common.Infrastructure.Data;

using Dapper;
using System.Data;
using System.Globalization;

internal sealed class TimeOnlyTypeHandler : SqlMapper.TypeHandler<TimeOnly>
{
    public override void SetValue(IDbDataParameter parameter, TimeOnly value)
    {
        parameter.DbType = DbType.Time;
        parameter.Value = value;
    }

    public override TimeOnly Parse(object value) => value switch
    {
        TimeOnly timeOnly => timeOnly,
        TimeSpan timeSpan => TimeOnly.FromTimeSpan(timeSpan),
        DateTime dateTime => TimeOnly.FromDateTime(dateTime),
        string text => TimeOnly.Parse(text, CultureInfo.InvariantCulture),
        _ => TimeOnly.FromTimeSpan(Convert.ToDateTime(value, CultureInfo.InvariantCulture).TimeOfDay)
    };
}
