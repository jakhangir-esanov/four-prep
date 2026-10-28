namespace FourPrep.Modules.Academy.Application.Enrolments.GetEnrolments;

using Dapper;
using System.Data.Common;
using FourPrep.Modules.Academy.Domain.Enrolments;

internal static class EnrolmentReader
{
    private const string SelectSql =
        $"""
        SELECT
            e.id AS {nameof(EnrolmentResponse.Id)},
            e.class_group_id AS {nameof(EnrolmentResponse.ClassGroupId)},
            g.code AS {nameof(EnrolmentResponse.ClassGroupCode)},
            c.slug AS {nameof(EnrolmentResponse.CourseSlug)},
            c.name AS {nameof(EnrolmentResponse.CourseName)},
            g.mode AS {nameof(EnrolmentResponse.Mode)},
            g.starts_on AS {nameof(EnrolmentResponse.StartsOn)},
            e.student_user_id AS {nameof(EnrolmentResponse.StudentUserId)},
            e.enrolled_by_user_id AS {nameof(EnrolmentResponse.EnrolledByUserId)},
            e.status AS {nameof(EnrolmentResponse.Status)},
            e.drop_reason AS {nameof(EnrolmentResponse.DropReason)},
            e.payment_due_on_utc AS {nameof(EnrolmentResponse.PaymentDueOnUtc)},
            CASE WHEN e.status = @Waitlisted THEN (
                SELECT COUNT(*)::int
                FROM academy.enrolments w
                WHERE w.class_group_id = e.class_group_id
                  AND w.status = @Waitlisted
                  AND (w.created_on_utc < e.created_on_utc
                       OR (w.created_on_utc = e.created_on_utc AND w.id <= e.id)))
            END AS {nameof(EnrolmentResponse.WaitlistPosition)},
            e.created_on_utc AS {nameof(EnrolmentResponse.CreatedOnUtc)}
        FROM academy.enrolments e
        JOIN academy.class_groups g ON g.id = e.class_group_id
        JOIN academy.courses c ON c.id = g.course_id
        """;

    public static Task<IReadOnlyList<EnrolmentResponse>> ReadForStudentAsync(
        DbConnection connection,
        Guid studentUserId,
        CancellationToken cancellationToken) =>
        ReadAsync(
            connection,
            $"{SelectSql} WHERE e.student_user_id = @StudentUserId ORDER BY e.created_on_utc DESC",
            new { StudentUserId = studentUserId, Waitlisted = (int)EnrolmentStatus.Waitlisted },
            cancellationToken);

    public static Task<IReadOnlyList<EnrolmentResponse>> ReadForClassGroupAsync(
        DbConnection connection,
        Guid classGroupId,
        CancellationToken cancellationToken) =>
        ReadAsync(
            connection,
            $"{SelectSql} WHERE e.class_group_id = @ClassGroupId ORDER BY e.status, e.created_on_utc, e.id",
            new { ClassGroupId = classGroupId, Waitlisted = (int)EnrolmentStatus.Waitlisted },
            cancellationToken);

    private static async Task<IReadOnlyList<EnrolmentResponse>> ReadAsync(
        DbConnection connection,
        string sql,
        object param,
        CancellationToken cancellationToken) =>
        (await connection.QueryAsync<EnrolmentResponse>(
            new CommandDefinition(sql, param, cancellationToken: cancellationToken))).AsList();
}
