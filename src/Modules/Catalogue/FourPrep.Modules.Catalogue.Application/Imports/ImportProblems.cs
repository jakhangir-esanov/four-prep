namespace FourPrep.Modules.Catalogue.Application.Imports;

using FourPrep.Common.Domain;
using System.Globalization;
using FluentValidation.Results;
using System.Text.RegularExpressions;
using FourPrep.Common.Application.Behaviors;
using FourPrep.Modules.Catalogue.Domain.Imports;

internal sealed partial class ImportProblems
{
    private readonly List<Error> _errors = [];

    public bool Any => _errors.Count > 0;

    public static string RowPath(int rowNumber, string column) => $"rows[{rowNumber}].{column}";

    public void Add(int rowNumber, string column, Error error) =>
        _errors.Add(new FieldError(error.Code, error.Messages, RowPath(rowNumber, column)));

    public void AddColumn(string column, Error error) =>
        _errors.Add(new FieldError(error.Code, error.Messages, $"columns.{column}"));

    public void AddFailures(ValidationResult result, int rowNumber) =>
        AddFailures(result, [rowNumber], null);

    public void AddFailures(ValidationResult result, IReadOnlyList<int> rowNumbers, string? listColumn)
    {
        foreach (var failure in result.Errors)
        {
            var path = ItemPath().Match(failure.PropertyName);
            int? index = path.Groups["index"].Success
                ? int.Parse(path.Groups["index"].Value, CultureInfo.InvariantCulture)
                : null;

            int rowNumber = index is int item && item < rowNumbers.Count ? rowNumbers[item] : rowNumbers[0];
            string column = index is null && listColumn is not null
                ? listColumn
                : ToColumn(path.Groups["property"].Value);

            _errors.Add(failure.ToFieldError(RowPath(rowNumber, column)));
        }
    }

    public void AddDuplicates<TKey>(IEnumerable<(int RowNumber, TKey Key)> keys, string column)
    {
        var seen = new HashSet<TKey>();

        foreach (var (rowNumber, _) in keys.Where(entry => !seen.Add(entry.Key)))
        {
            Add(rowNumber, column, ImportErrors.DuplicateRow);
        }
    }

    public ValidationError ToError() => new([.. _errors]);

    private static string ToColumn(string property) =>
        WordBoundary().Replace(ItemIndex().Replace(property, string.Empty), "$1_$2").ToLowerInvariant();

    [GeneratedRegex(@"^(?:[A-Za-z]+\[(?<index>\d+)\]\.)?(?<property>.+)$")]
    private static partial Regex ItemPath();

    [GeneratedRegex(@"\[\d+\]")]
    private static partial Regex ItemIndex();

    [GeneratedRegex("([a-z0-9])([A-Z])")]
    private static partial Regex WordBoundary();
}
