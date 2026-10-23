namespace FourPrep.Modules.Student.Presentation.Students.Requests;

using FourPrep.Common.Domain.Enums;

internal sealed record GetStudentsRequest
{
    public int? First { get; init; }
    public int? Rows { get; init; }
    public string? SortField { get; init; }
    public int? SortOrder { get; init; }

    public string? Search { get; init; }
    public Region? Region { get; init; }
    public int? Grade { get; init; }
    public Exam? Exam { get; init; }
    public Guid? MentorUserId { get; init; }
    public bool? HasMentor { get; init; }
}
