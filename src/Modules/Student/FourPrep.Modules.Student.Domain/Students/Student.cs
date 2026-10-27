namespace FourPrep.Modules.Student.Domain.Students;

using FourPrep.Modules.Student.Domain.ExamGoals;

public sealed class Student
{
    private readonly List<ExamGoal> _examGoals = [];

    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? School { get; set; }
    public int? Grade { get; set; }
    public string? Country { get; set; }
    public string? Region { get; set; }
    public Guid? MentorUserId { get; set; }
    public string LinkCode { get; set; } = string.Empty;
    public IReadOnlyCollection<ExamGoal> ExamGoals => _examGoals;
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset UpdatedOnUtc { get; set; }

    public static Student Create(Guid userId, string userName, string? country, string linkCode, DateTime utcNow)
    {
        return new Student
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UserName = userName,
            Country = country,
            LinkCode = linkCode,
            CreatedOnUtc = utcNow,
            UpdatedOnUtc = utcNow
        };
    }

    public void UpdateProfile(
        string? firstName,
        string? lastName,
        DateOnly? dateOfBirth,
        string? school,
        int? grade,
        string? country,
        string? region,
        IEnumerable<ExamGoal> examGoals,
        DateTime utcNow)
    {
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        School = school;
        Grade = grade;
        Country = country;
        Region = region;
        _examGoals.Clear();
        _examGoals.AddRange(examGoals);
        UpdatedOnUtc = utcNow;
    }

    public void AssignMentor(Guid mentorUserId, DateTime utcNow)
    {
        MentorUserId = mentorUserId;
        UpdatedOnUtc = utcNow;
    }

    public void RemoveMentor(DateTime utcNow)
    {
        MentorUserId = null;
        UpdatedOnUtc = utcNow;
    }

    public void ChangeLinkCode(string linkCode, DateTime utcNow)
    {
        LinkCode = linkCode;
        UpdatedOnUtc = utcNow;
    }
}
