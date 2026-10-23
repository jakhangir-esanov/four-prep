namespace FourPrep.Modules.Student.Application.Students.GetStudents;

using FourPrep.Common.Domain.Enums;

public sealed record StudentSummaryResponse
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? School { get; init; }
    public int? Grade { get; init; }
    public Region? Region { get; init; }
    public Guid? MentorUserId { get; init; }
    public DateTime CreatedOnUtc { get; init; }
}
