namespace FourPrep.Modules.Student.Application.StudentParents.GetMyParents;

using Dapper;
using FourPrep.Common.Domain;
using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Student.Application.Abstractions.Authentication;

public sealed record GetMyParentsQuery : IQuery<IReadOnlyList<ParentResponse>>;

internal sealed class GetMyParentsQueryHandler(
    ICurrentUser currentUser,
    IDbConnectionFactory dbConnectionFactory) : IQueryHandler<GetMyParentsQuery, IReadOnlyList<ParentResponse>>
{
    public async Task<Result<IReadOnlyList<ParentResponse>>> Handle(
        GetMyParentsQuery request,
        CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            $"""
            SELECT
                sp.parent_user_id AS {nameof(ParentResponse.ParentUserId)},
                sp.parent_user_name AS {nameof(ParentResponse.ParentUserName)},
                sp.linked_on_utc AS {nameof(ParentResponse.LinkedOnUtc)}
            FROM student.student_parents sp
            JOIN student.students s ON s.id = sp.student_id
            WHERE s.user_id = @UserId
            ORDER BY sp.linked_on_utc
            """;

        var parents = await connection.QueryAsync<ParentResponse>(
            new CommandDefinition(sql, new { currentUser.UserId }, cancellationToken: cancellationToken));

        return Result.Success<IReadOnlyList<ParentResponse>>(parents.AsList());
    }
}
