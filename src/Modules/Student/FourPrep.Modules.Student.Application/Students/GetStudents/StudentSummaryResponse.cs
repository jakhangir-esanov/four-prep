namespace FourPrep.Modules.Student.Application.Students.GetStudents;

public sealed record StudentSummaryResponse
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? School { get; init; }
    public int? Grade { get; init; }
    public string? Country { get; init; }
    public string? Region { get; init; }
    public Guid? MentorUserId { get; init; }
    public DateTime CreatedOnUtc { get; init; }
}
