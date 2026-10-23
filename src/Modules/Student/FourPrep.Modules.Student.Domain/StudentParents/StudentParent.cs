namespace FourPrep.Modules.Student.Domain.StudentParents;

public sealed class StudentParent
{
    public const int MaxParentsPerStudent = 4;

    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Guid ParentUserId { get; set; }
    public string ParentUserName { get; set; } = string.Empty;
    public DateTimeOffset LinkedOnUtc { get; set; }

    public static StudentParent Create(Guid studentId, Guid parentUserId, string parentUserName, DateTime utcNow)
    {
        return new StudentParent
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,
            ParentUserId = parentUserId,
            ParentUserName = parentUserName,
            LinkedOnUtc = utcNow
        };
    }
}
