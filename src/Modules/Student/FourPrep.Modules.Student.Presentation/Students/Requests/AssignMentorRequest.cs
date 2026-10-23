namespace FourPrep.Modules.Student.Presentation.Students.Requests;

internal sealed record AssignMentorRequest
{
    public Guid MentorUserId { get; init; }
}
