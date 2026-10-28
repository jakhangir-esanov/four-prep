namespace FourPrep.Modules.Catalogue.Infrastructure.Csv;

using CsvHelper;
using System.Text;
using System.Globalization;
using CsvHelper.Configuration;
using FourPrep.Common.Domain;
using FourPrep.Modules.Catalogue.Domain.Imports;
using FourPrep.Modules.Catalogue.Application.Abstractions.Csv;

internal sealed class CsvFileReader : ICsvReader
{
    private static readonly CsvConfiguration Configuration = new(CultureInfo.InvariantCulture)
    {
        DetectDelimiter = true,
        TrimOptions = TrimOptions.Trim,
        IgnoreBlankLines = true,
        MissingFieldFound = null
    };

    public Result<CsvTable> Read(Stream stream)
    {
        try
        {
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            using var csv = new CsvReader(reader, Configuration);

            if (!csv.Read() || !csv.ReadHeader() || csv.HeaderRecord is null)
            {
                return Result.Failure<CsvTable>(ImportErrors.EmptyFile);
            }

            string[] columns = [.. csv.HeaderRecord.Select(header => header.Trim().ToLowerInvariant())];
            if (columns.Distinct(StringComparer.Ordinal).Count() != columns.Length)
            {
                return Result.Failure<CsvTable>(ImportErrors.UnreadableFile);
            }

            return new CsvTable(columns, ReadRows(csv, columns));
        }
        catch (CsvHelperException)
        {
            return Result.Failure<CsvTable>(ImportErrors.UnreadableFile);
        }
    }

    private static List<CsvRow> ReadRows(CsvReader csv, string[] columns)
    {
        var rows = new List<CsvRow>();

        while (csv.Read())
        {
            var values = new Dictionary<string, string>(StringComparer.Ordinal);
            for (int index = 0; index < columns.Length; index++)
            {
                values[columns[index]] = csv.GetField(index) ?? string.Empty;
            }

            rows.Add(new CsvRow(csv.Parser.Row, values));
        }

        return rows;
    }
}
