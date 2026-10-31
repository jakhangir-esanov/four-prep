namespace FourPrep.Modules.Student.Application.Students.GetStudentContact;

using Dapper;
using FourPrep.Common.Domain;
using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Student.Domain.Students;

public sealed record GetStudentContactQuery(Guid StudentUserId) : IQuery<StudentContactResponse>;

internal sealed class GetStudentContactQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetStudentContactQuery, StudentContactResponse>
{
    private const string Sql =
        $"""
        SELECT
            s.user_id AS {nameof(StudentContactResponse.UserId)},
            COALESCE(NULLIF(TRIM(CONCAT_WS(' ', s.first_name, s.last_name)), ''), s.user_name)
                AS {nameof(StudentContactResponse.DisplayName)},
            s.mentor_user_id AS {nameof(StudentContactResponse.MentorUserId)},
            s.date_of_birth AS {nameof(StudentContactResponse.DateOfBirth)}
        FROM student.students s
        WHERE s.user_id = @StudentUserId
        """;

    public async Task<Result<StudentContactResponse>> Handle(
        GetStudentContactQuery request,
        CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        var contact = await connection.QuerySingleOrDefaultAsync<StudentContactResponse>(
            new CommandDefinition(Sql, new { request.StudentUserId }, cancellationToken: cancellationToken));

        return contact is null
            ? Result.Failure<StudentContactResponse>(StudentErrors.NotFound)
            : contact;
    }
}
