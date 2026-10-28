namespace FourPrep.Modules.Academy.Presentation.ClassGroups.Requests;

internal sealed record AssignTeacherRequest
{
    public Guid TeacherUserId { get; init; }
    public string? TeacherName { get; init; }
}
