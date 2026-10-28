namespace FourPrep.Modules.Student.Application.Students.HasFamilyAccess;

using Dapper;
using FourPrep.Common.Domain;
using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Messaging;

public sealed record HasFamilyAccessQuery(Guid UserId, Guid StudentUserId) : IQuery<bool>;

internal sealed class HasFamilyAccessQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<HasFamilyAccessQuery, bool>
{
    private const string Sql =
        """
        SELECT EXISTS (
            SELECT 1
            FROM student.students s
            WHERE s.user_id = @StudentUserId
              AND (s.user_id = @UserId
                   OR EXISTS (
                       SELECT 1
                       FROM student.student_parents sp
                       WHERE sp.student_id = s.id AND sp.parent_user_id = @UserId)))
        """;

    public async Task<Result<bool>> Handle(HasFamilyAccessQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        var hasAccess = await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(
                Sql,
                new { request.UserId, request.StudentUserId },
                cancellationToken: cancellationToken));

        return Result.Success(hasAccess);
    }
}
