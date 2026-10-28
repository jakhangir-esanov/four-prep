namespace FourPrep.Modules.Catalogue.Application.Imports;

using FourPrep.Modules.Catalogue.Application.Abstractions.Csv;

internal sealed record ImportLine<TInput>(int RowNumber, string UniversitySlug, string? ProgramName, TInput Input);

internal sealed record ImportGroup<TInput>(
    string UniversitySlug,
    string? ProgramName,
    IReadOnlyList<int> RowNumbers,
    IReadOnlyList<TInput> Inputs);

internal static class ImportLines
{
    public static List<ImportLine<TInput>> ForUniversities<TInput>(
        CsvTable table,
        ImportProblems problems,
        Func<ImportRow, TInput> parse) =>
        Parse(table, problems, row => null, parse);

    public static List<ImportLine<TInput>> ForPrograms<TInput>(
        CsvTable table,
        ImportProblems problems,
        Func<ImportRow, TInput> parse) =>
        Parse(table, problems, row => row.Text(ImportColumns.ProgramName), parse);

    public static List<ImportGroup<TInput>> Group<TInput>(IEnumerable<ImportLine<TInput>> lines) =>
        [.. lines
            .GroupBy(line => (line.UniversitySlug, line.ProgramName))
            .Select(group => new ImportGroup<TInput>(
                group.Key.UniversitySlug,
                group.Key.ProgramName,
                [.. group.Select(line => line.RowNumber)],
                [.. group.Select(line => line.Input)]))];

    private static List<ImportLine<TInput>> Parse<TInput>(
        CsvTable table,
        ImportProblems problems,
        Func<ImportRow, string?> programName,
        Func<ImportRow, TInput> parse) =>
        [.. table.Rows
            .Select(csvRow => new ImportRow(csvRow, problems))
            .Select(row => new ImportLine<TInput>(row.Number, row.Key(ImportColumns.UniversitySlug), programName(row), parse(row)))];
}
