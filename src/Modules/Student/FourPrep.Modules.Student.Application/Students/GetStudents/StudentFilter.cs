namespace FourPrep.Modules.Student.Application.Students.GetStudents;

using FourPrep.Common.Domain.Enums;

public sealed record StudentFilter(
    string? Search = null,
    Region? Region = null,
    int? Grade = null,
    Exam? Exam = null,
    Guid? MentorUserId = null,
    bool? HasMentor = null)
{
    public static readonly StudentFilter None = new();

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
