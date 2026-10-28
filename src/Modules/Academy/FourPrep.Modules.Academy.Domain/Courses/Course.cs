namespace FourPrep.Modules.Academy.Domain.Courses;

public sealed class Course
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public Dictionary<string, string> Name { get; set; } = [];
    public Dictionary<string, string> Description { get; set; } = [];
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset UpdatedOnUtc { get; set; }

    public static Course Create(
        string slug,
        Dictionary<string, string> name,
        Dictionary<string, string> description,
        DateTime utcNow)
    {
        var course = new Course
        {
            Id = Guid.NewGuid(),
            CreatedOnUtc = utcNow
        };

        course.Update(slug, name, description, utcNow);

        return course;
    }

    public void Update(
        string slug,
        Dictionary<string, string> name,
        Dictionary<string, string> description,
        DateTime utcNow)
    {
        Slug = slug;
        Name = name;
        Description = description;
        UpdatedOnUtc = utcNow;
    }
}
