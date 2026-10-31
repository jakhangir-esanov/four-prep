namespace FourPrep.Modules.Student.Application.Students.GetMenteeUserIds;

using Dapper;
using FourPrep.Common.Domain;
using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Messaging;

public sealed record GetMenteeUserIdsQuery(Guid MentorUserId) : IQuery<IReadOnlyList<Guid>>;

internal sealed class GetMenteeUserIdsQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetMenteeUserIdsQuery, IReadOnlyList<Guid>>
{
    private const string Sql =
        """
        SELECT s.user_id
        FROM student.students s
        WHERE s.mentor_user_id = @MentorUserId
        """;

    public async Task<Result<IReadOnlyList<Guid>>> Handle(GetMenteeUserIdsQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        var userIds = await connection.QueryAsync<Guid>(
            new CommandDefinition(Sql, new { request.MentorUserId }, cancellationToken: cancellationToken));

        return Result.Success<IReadOnlyList<Guid>>(userIds.AsList());
    }
}
