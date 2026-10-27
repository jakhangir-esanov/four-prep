namespace FourPrep.Modules.Student.Application.Students.GetStudents;

using FourPrep.Common.Domain.Enums;
using FourPrep.Common.Domain.Localization;

public sealed record StudentFilter(
    string? Search = null,
    string? Country = null,
    int? Grade = null,
    Exam? Exam = null,
    Guid? MentorUserId = null,
    bool? HasMentor = null)
{
    public static readonly StudentFilter None = new();

    public string? CountryCode => Countries.NormalizeOrNull(Country);

    public string? SearchPattern
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Search))
            {
                return null;
            }

            string escaped = Search.Trim()
                .Replace(@"\", @"\\")
                .Replace("%", @"\%")
                .Replace("_", @"\_");

            return $"%{escaped}%";
        }
    }
}
