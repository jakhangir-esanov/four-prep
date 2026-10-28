namespace FourPrep.Modules.Academy.Application.Courses.GetCourses;

using Dapper;
using System.Data.Common;
using FourPrep.Modules.Academy.Domain.ClassGroups;

internal static class CourseReader
{
    private const string SelectSql =
        $"""
        SELECT
            c.id AS {nameof(CourseResponse.Id)},
            c.slug AS {nameof(CourseResponse.Slug)},
            c.name AS {nameof(CourseResponse.Name)},
            c.description AS {nameof(CourseResponse.Description)},
            (SELECT COUNT(*) FROM academy.class_groups g WHERE g.course_id = c.id AND g.status = @Open)::int
                AS {nameof(CourseResponse.OpenGroupCount)}
        FROM academy.courses c
        """;

    public static async Task<IReadOnlyList<CourseResponse>> ReadAllAsync(
        DbConnection connection,
        CancellationToken cancellationToken)
    {
        const string sql = $"{SelectSql} ORDER BY c.slug";

        return (await connection.QueryAsync<CourseResponse>(
            new CommandDefinition(sql, new { Open = (int)ClassGroupStatus.Open }, cancellationToken: cancellationToken)))
            .AsList();
    }

    public static async Task<CourseResponse?> ReadBySlugAsync(
        DbConnection connection,
        string slug,
        CancellationToken cancellationToken)
    {
        const string sql = $"{SelectSql} WHERE c.slug = @Slug";

        return await connection.QuerySingleOrDefaultAsync<CourseResponse>(
            new CommandDefinition(
                sql,
                new { Slug = slug, Open = (int)ClassGroupStatus.Open },
                cancellationToken: cancellationToken));
    }
}
