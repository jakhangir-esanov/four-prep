namespace FourPrep.Modules.Academy.Application.Courses.GetCourses;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Messaging;

public sealed record GetCoursesQuery : IQuery<IReadOnlyList<CourseResponse>>;

internal sealed class GetCoursesQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetCoursesQuery, IReadOnlyList<CourseResponse>>
{
    public async Task<Result<IReadOnlyList<CourseResponse>>> Handle(
        GetCoursesQuery request,
        CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        return Result.Success(await CourseReader.ReadAllAsync(connection, cancellationToken));
    }
}
