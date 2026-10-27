namespace FourPrep.Modules.Student.Application.Students.GetStudent;

using FourPrep.Common.Domain.Enums;

public record StudentResponse
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public DateOnly? DateOfBirth { get; init; }
    public string? School { get; init; }
    public int? Grade { get; init; }
    public string? Country { get; init; }
    public string? Region { get; init; }
    public Guid? MentorUserId { get; init; }
    public IReadOnlyList<ExamGoalResponse> ExamGoals { get; init; } = [];
    public DateTime CreatedOnUtc { get; init; }
    public DateTime UpdatedOnUtc { get; init; }
}

public sealed record MyStudentResponse : StudentResponse
{
    public string LinkCode { get; init; } = string.Empty;
}

public sealed record ExamGoalResponse
{
    public Exam Exam { get; init; }
    public decimal TargetScore { get; init; }
}
