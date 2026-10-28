namespace FourPrep.Modules.Catalogue.Application.Abstractions.Csv;

public sealed record CsvTable(IReadOnlyList<string> Columns, IReadOnlyList<CsvRow> Rows);

public sealed record CsvRow(int Number, IReadOnlyDictionary<string, string> Values);
