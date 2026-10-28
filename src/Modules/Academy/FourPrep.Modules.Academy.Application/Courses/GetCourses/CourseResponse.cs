namespace FourPrep.Modules.Academy.Application.Courses.GetCourses;

public sealed record CourseResponse
{
    public Guid Id { get; init; }
    public string Slug { get; init; } = string.Empty;
    public Dictionary<string, string> Name { get; init; } = [];
    public Dictionary<string, string> Description { get; init; } = [];
    public int OpenGroupCount { get; init; }
}
