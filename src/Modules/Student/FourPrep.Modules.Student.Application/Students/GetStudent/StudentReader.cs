namespace FourPrep.Modules.Student.Application.Students.GetStudent;

using Dapper;
using System.Data.Common;

internal static class StudentReader
{
    public static async Task<StudentResponse?> ReadByIdAsync(
        DbConnection connection,
        Guid id,
        CancellationToken cancellationToken) =>
        await ReadAsync(connection, "id = @Value", id, cancellationToken);

    public static async Task<StudentResponse?> ReadByUserIdAsync(
        DbConnection connection,
        Guid userId,
        CancellationToken cancellationToken) =>
        await ReadAsync(connection, "user_id = @Value", userId, cancellationToken);

    private static async Task<StudentResponse?> ReadAsync(
        DbConnection connection,
        string condition,
        Guid value,
        CancellationToken cancellationToken)
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
                region AS {nameof(StudentResponse.Region)},
                mentor_user_id AS {nameof(StudentResponse.MentorUserId)},
                link_code AS {nameof(StudentResponse.LinkCode)},
                created_on_utc AS {nameof(StudentResponse.CreatedOnUtc)},
                updated_on_utc AS {nameof(StudentResponse.UpdatedOnUtc)}
            FROM
                student.students
            WHERE
                {condition}
            """;

        var student = await connection.QuerySingleOrDefaultAsync<StudentResponse>(
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

        return student with { ExamGoals = examGoals.AsList() };
    }
}
