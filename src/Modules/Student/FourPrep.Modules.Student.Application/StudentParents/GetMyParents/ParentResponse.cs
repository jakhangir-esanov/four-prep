namespace FourPrep.Modules.Student.Application.StudentParents.GetMyParents;

public sealed record ParentResponse
{
    public Guid ParentUserId { get; init; }
    public string ParentUserName { get; init; } = string.Empty;
    public DateTime LinkedOnUtc { get; init; }
}
