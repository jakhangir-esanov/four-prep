namespace FourPrep.Common.Infrastructure.Data;

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public static class JsonColumnExtensions
{
    private const string JsonbColumnType = "jsonb";

    public static PropertyBuilder<T> HasJsonbConversion<T>(this PropertyBuilder<T> property)
        where T : class, new()
    {
        var converter = new ValueConverter<T, string>(
            value => Serialize(value),
            json => Deserialize<T>(json));

        var comparer = new ValueComparer<T>(
            (left, right) => Serialize(left) == Serialize(right),
            value => Serialize(value).GetHashCode(),
            value => Deserialize<T>(Serialize(value)));

        return property
            .HasConversion(converter, comparer)
            .HasColumnType(JsonbColumnType);
    }

    private static string Serialize<T>(T? value) => JsonSerializer.Serialize(value);

    private static T Deserialize<T>(string json)
        where T : class, new() =>
        JsonSerializer.Deserialize<T>(json) ?? new T();
}
