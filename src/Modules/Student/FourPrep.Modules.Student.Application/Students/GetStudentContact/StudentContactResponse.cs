namespace FourPrep.Modules.Student.Application.Students.GetStudentContact;

public sealed record StudentContactResponse
{
    public Guid UserId { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public Guid? MentorUserId { get; init; }
    public DateOnly? DateOfBirth { get; init; }
}
