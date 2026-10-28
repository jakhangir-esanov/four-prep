namespace FourPrep.Modules.Academy.Application.Courses.GetCourses;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Academy.Domain.Courses;

public sealed record GetCourseQuery(string Slug) : IQuery<CourseResponse>;

internal sealed class GetCourseQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetCourseQuery, CourseResponse>
{
    public async Task<Result<CourseResponse>> Handle(GetCourseQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        var course = await CourseReader.ReadBySlugAsync(
            connection,
            request.Slug.Trim().ToLowerInvariant(),
            cancellationToken);

        return course is null
            ? Result.Failure<CourseResponse>(CourseErrors.NotFound)
            : course;
    }
}
