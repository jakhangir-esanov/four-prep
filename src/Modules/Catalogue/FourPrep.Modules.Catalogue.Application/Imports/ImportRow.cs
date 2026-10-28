namespace FourPrep.Modules.Catalogue.Application.Imports;

using System.Globalization;
using FourPrep.Common.Domain.Enums;
using FourPrep.Modules.Catalogue.Domain.Imports;
using FourPrep.Modules.Catalogue.Application.Abstractions.Csv;

internal sealed class ImportRow(CsvRow row, ImportProblems problems)
{
    private const char ListSeparator = ';';
    private const string DateFormat = "yyyy-MM-dd";

    public int Number => row.Number;

    public string Text(string column) => Value(column) ?? string.Empty;

    public string? OptionalText(string column) => Value(column);

    public string Key(string column) => Text(column).ToLowerInvariant();

    public int? Int(string column) =>
        Parse<int>(column, text => int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) ? value : null);

    public long? Long(string column) =>
        Parse<long>(column, text => long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out long value) ? value : null);

    public decimal? Decimal(string column) =>
        Parse<decimal>(column, text => decimal.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal value) ? value : null);

    public bool? Bool(string column) =>
        Parse<bool>(column, text => bool.TryParse(text, out bool value) ? value : null);

    public DateOnly? Date(string column) =>
        Parse<DateOnly>(column, text => DateOnly.TryParseExact(text, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var value) ? value : null);

    public TEnum? Choice<TEnum>(string column)
        where TEnum : struct, Enum =>
        Parse<TEnum>(column, text => Enum.TryParse(text, ignoreCase: true, out TEnum value) && Enum.IsDefined(value) ? value : null);

    public Dictionary<string, string> Localized(string column)
    {
        string prefix = column + ImportLayout.LocaleSeparator;

        return row.Values
            .Where(entry => entry.Key.StartsWith(prefix, StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(entry.Value))
            .ToDictionary(entry => entry.Key[prefix.Length..], entry => entry.Value.Trim());
    }

    public Dictionary<Exam, decimal> ExamMinimums()
    {
        var minimums = new Dictionary<Exam, decimal>();

        foreach (string column in row.Values.Keys)
        {
            if (ImportLayout.TryGetExam(column, out Exam exam) && Decimal(column) is decimal score)
            {
                minimums[exam] = score;
            }
        }

        return minimums;
    }

    public List<string> List(string column) =>
        [.. Text(column).Split(ListSeparator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)];

    private string? Value(string column) =>
        row.Values.TryGetValue(column, out string? value) && !string.IsNullOrWhiteSpace(value) ? value.Trim() : null;

    private T? Parse<T>(string column, Func<string, T?> parse)
        where T : struct
    {
        string? text = Value(column);
        if (text is null)
        {
            return null;
        }

        T? value = parse(text);
        if (value is null)
        {
            problems.Add(Number, column, ImportErrors.InvalidValue);
        }

        return value;
    }
}
