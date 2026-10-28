namespace FourPrep.Modules.Catalogue.Application.Imports;

using FourPrep.Common.Domain.Enums;
using FourPrep.Modules.Catalogue.Domain.Imports;

internal sealed record ImportLayout(
    IReadOnlyList<string> RequiredColumns,
    IReadOnlyList<string> OptionalColumns,
    IReadOnlyList<string>? LocalizedColumns = null,
    bool AcceptsExamMinimums = false)
{
    public const string ExamMinimumSuffix = "_min";
    public const char LocaleSeparator = '_';

    public void Check(IReadOnlyList<string> columns, ImportProblems problems)
    {
        foreach (string missing in RequiredColumns.Except(columns))
        {
            problems.AddColumn(missing, ImportErrors.MissingColumn);
        }

        foreach (string unknown in columns.Where(column => !IsKnown(column)))
        {
            problems.AddColumn(unknown, ImportErrors.UnknownColumn);
        }
    }

    public static bool TryGetExam(string column, out Exam exam)
    {
        exam = default;

        return column.EndsWith(ExamMinimumSuffix, StringComparison.Ordinal)
            && Enum.TryParse(column[..^ExamMinimumSuffix.Length], ignoreCase: true, out exam)
            && Enum.IsDefined(exam);
    }

    private bool IsKnown(string column) =>
        RequiredColumns.Contains(column)
        || OptionalColumns.Contains(column)
        || (LocalizedColumns ?? []).Any(prefix => column.StartsWith(prefix + LocaleSeparator, StringComparison.Ordinal))
        || (AcceptsExamMinimums && TryGetExam(column, out _));
}
