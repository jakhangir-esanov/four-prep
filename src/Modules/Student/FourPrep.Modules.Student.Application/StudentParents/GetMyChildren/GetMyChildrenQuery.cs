namespace FourPrep.Modules.Student.Application.StudentParents.GetMyChildren;

using Dapper;
using FourPrep.Common.Domain;
using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Student.Application.Students.GetStudents;
using FourPrep.Modules.Student.Application.Abstractions.Authentication;

public sealed record GetMyChildrenQuery : IQuery<IReadOnlyList<StudentSummaryResponse>>;

internal sealed class GetMyChildrenQueryHandler(
    ICurrentUser currentUser,
    IDbConnectionFactory dbConnectionFactory) : IQueryHandler<GetMyChildrenQuery, IReadOnlyList<StudentSummaryResponse>>
{
    public async Task<Result<IReadOnlyList<StudentSummaryResponse>>> Handle(
        GetMyChildrenQuery request,
        CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            $"""
            SELECT
                s.id AS {nameof(StudentSummaryResponse.Id)},
                s.user_id AS {nameof(StudentSummaryResponse.UserId)},
                s.user_name AS {nameof(StudentSummaryResponse.UserName)},
                s.first_name AS {nameof(StudentSummaryResponse.FirstName)},
                s.last_name AS {nameof(StudentSummaryResponse.LastName)},
                s.school AS {nameof(StudentSummaryResponse.School)},
                s.grade AS {nameof(StudentSummaryResponse.Grade)},
                s.country AS {nameof(StudentSummaryResponse.Country)},
                s.region AS {nameof(StudentSummaryResponse.Region)},
                s.mentor_user_id AS {nameof(StudentSummaryResponse.MentorUserId)},
                s.created_on_utc AS {nameof(StudentSummaryResponse.CreatedOnUtc)}
            FROM student.student_parents sp
            JOIN student.students s ON s.id = sp.student_id
            WHERE sp.parent_user_id = @ParentUserId
            ORDER BY sp.linked_on_utc
            """;

        var children = await connection.QueryAsync<StudentSummaryResponse>(
            new CommandDefinition(sql, new { ParentUserId = currentUser.UserId }, cancellationToken: cancellationToken));

        return Result.Success<IReadOnlyList<StudentSummaryResponse>>(children.AsList());
    }
}
