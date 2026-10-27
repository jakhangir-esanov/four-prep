namespace FourPrep.Modules.Student.Application.Students.GetStudent;

using Dapper;
using System.Data.Common;

internal static class StudentReader
{
    private const string OwnerOnlyColumns = $"link_code AS {nameof(MyStudentResponse.LinkCode)},";

    public static async Task<StudentResponse?> ReadByIdAsync(
        DbConnection connection,
        Guid id,
        CancellationToken cancellationToken) =>
        await ReadAsync<StudentResponse>(connection, "id = @Value", id, string.Empty, cancellationToken);

    public static async Task<MyStudentResponse?> ReadOwnProfileAsync(
        DbConnection connection,
        Guid userId,
        CancellationToken cancellationToken) =>
        await ReadAsync<MyStudentResponse>(connection, "user_id = @Value", userId, OwnerOnlyColumns, cancellationToken);

    private static async Task<TResponse?> ReadAsync<TResponse>(
        DbConnection connection,
        string condition,
        Guid value,
        string extraColumns,
        CancellationToken cancellationToken)
        where TResponse : StudentResponse
    {
        var studentSql =
            $"""
            SELECT
                id AS {nameof(StudentResponse.Id)},
                user_id AS {nameof(StudentResponse.UserId)},
                user_name AS {nameof(StudentResponse.UserName)},
                first_name AS {nameof(StudentResponse.FirstName)},
                last_name AS {nameof(StudentResponse.LastName)},
                date_of_birth AS {nameof(StudentResponse.DateOfBirth)},
                school AS {nameof(StudentResponse.School)},
                grade AS {nameof(StudentResponse.Grade)},
                country AS {nameof(StudentResponse.Country)},
                region AS {nameof(StudentResponse.Region)},
                mentor_user_id AS {nameof(StudentResponse.MentorUserId)},
                {extraColumns}
                created_on_utc AS {nameof(StudentResponse.CreatedOnUtc)},
                updated_on_utc AS {nameof(StudentResponse.UpdatedOnUtc)}
            FROM
                student.students
            WHERE
                {condition}
            """;

        var student = await connection.QuerySingleOrDefaultAsync<TResponse>(
            new CommandDefinition(studentSql, new { Value = value }, cancellationToken: cancellationToken));

        if (student is null)
        {
            return null;
        }

        const string examGoalsSql =
            $"""
            SELECT
                exam AS {nameof(ExamGoalResponse.Exam)},
                target_score AS {nameof(ExamGoalResponse.TargetScore)}
            FROM
                student.exam_goals
            WHERE
                student_id = @StudentId
            ORDER BY exam
            """;

        var examGoals = await connection.QueryAsync<ExamGoalResponse>(
            new CommandDefinition(examGoalsSql, new { StudentId = student.Id }, cancellationToken: cancellationToken));

        StudentResponse profile = student;
        return (TResponse)(profile with { ExamGoals = examGoals.AsList() });
    }
}
