namespace FourPrep.Modules.Academy.Application.Courses;

using FourPrep.Common.Domain.Localization;
using FourPrep.Modules.Academy.Domain.Courses;

public sealed record CourseDetails(
    string Slug,
    IReadOnlyDictionary<string, string>? Name,
    IReadOnlyDictionary<string, string>? Description)
{
    public string NormalizedSlug => Slug.Trim();

    internal Course ToCourse(DateTime utcNow) =>
        Course.Create(NormalizedSlug, LocalizedTexts.Normalize(Name), LocalizedTexts.Normalize(Description), utcNow);

    internal void ApplyTo(Course course, DateTime utcNow) =>
        course.Update(NormalizedSlug, LocalizedTexts.Normalize(Name), LocalizedTexts.Normalize(Description), utcNow);
}
