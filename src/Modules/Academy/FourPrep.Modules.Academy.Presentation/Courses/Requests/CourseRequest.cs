namespace FourPrep.Modules.Academy.Presentation.Courses.Requests;

using FourPrep.Modules.Academy.Application.Courses;

internal sealed record CourseRequest
{
    public string? Slug { get; init; }
    public Dictionary<string, string>? Name { get; init; }
    public Dictionary<string, string>? Description { get; init; }

    public CourseDetails ToDetails() => new(Slug ?? string.Empty, Name, Description);
}
