namespace FourPrep.Modules.Academy.Application.ClassGroups.GetClassGroups;

using FourPrep.Common.Application.Data;
using FourPrep.Modules.Academy.Domain.ClassGroups;

public sealed record ClassGroupFilter(
    string? Course = null,
    ClassMode? Mode = null,
    ClassGroupStatus? Status = null,
    Guid? TeacherUserId = null,
    string? Search = null)
{
    public static readonly ClassGroupFilter None = new();

    public string? CourseSlug => string.IsNullOrWhiteSpace(Course) ? null : Course.Trim().ToLowerInvariant();

    public string? SearchPattern => LikePatterns.Contains(Search);
}
