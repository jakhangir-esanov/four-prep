namespace FourPrep.Modules.Student.Presentation.Students.Requests;

using FourPrep.Common.Domain.Enums;

internal sealed record UpdateMyStudentRequest
{
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public DateOnly? DateOfBirth { get; init; }
    public string? School { get; init; }
    public int? Grade { get; init; }
    // ISO 3166-1 alpha-2 code, and the state, province or region as the student writes it.
    public string? Country { get; init; }
    public string? Region { get; init; }
    public IReadOnlyList<ExamGoalRequest>? ExamGoals { get; init; }
}

internal sealed record ExamGoalRequest
{
    public Exam Exam { get; init; }
    public decimal TargetScore { get; init; }
}
