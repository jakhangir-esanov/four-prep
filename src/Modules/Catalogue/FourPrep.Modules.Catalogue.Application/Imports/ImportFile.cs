namespace FourPrep.Modules.Catalogue.Application.Imports;

using FourPrep.Common.Domain;
using FourPrep.Modules.Catalogue.Domain.Imports;
using FourPrep.Modules.Catalogue.Application.Abstractions.Csv;

internal static class ImportFile
{
    public const int MaxRows = 5000;

    public static Result<CsvTable> Read(ICsvReader csvReader, Stream file, ImportLayout layout)
    {
        var table = csvReader.Read(file);
        if (table.IsFailure)
        {
            return table;
        }

        if (table.Data.Rows.Count == 0)
        {
            return Result.Failure<CsvTable>(ImportErrors.EmptyFile);
        }

        if (table.Data.Rows.Count > MaxRows)
        {
            return Result.Failure<CsvTable>(ImportErrors.TooManyRows);
        }

        var problems = new ImportProblems();
        layout.Check(table.Data.Columns, problems);

        return problems.Any ? Result.Failure<CsvTable>(problems.ToError()) : table;
    }
}
