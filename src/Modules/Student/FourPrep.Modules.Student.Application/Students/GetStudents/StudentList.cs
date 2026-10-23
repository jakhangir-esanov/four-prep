namespace FourPrep.Modules.Student.Application.Students.GetStudents;

using Dapper;
using System.Data.Common;
using FourPrep.Common.Application.Paging;

internal static class StudentList
{
    public static async Task<PagedList<StudentSummaryResponse>> ReadAsync(
        DbConnection connection,
        PagingRequest<StudentSummaryResponse> paging,
        StudentFilter filter,
        CancellationToken cancellationToken)
    {
        var conditions = new List<string>();

        if (filter.SearchPattern is not null)
        {
            conditions.Add("(s.first_name ILIKE @Search OR s.last_name ILIKE @Search OR s.user_name ILIKE @Search)");
        }

        if (filter.Region is not null)
        {
            conditions.Add("s.region = @Region");
        }

        if (filter.Grade is not null)
        {
            conditions.Add("s.grade = @Grade");
        }

        if (filter.Exam is not null)
        {
            conditions.Add("EXISTS (SELECT 1 FROM student.exam_goals g WHERE g.student_id = s.id AND g.exam = @Exam)");
        }

        if (filter.MentorUserId is not null)
        {
            conditions.Add("s.mentor_user_id = @MentorUserId");
        }

        if (filter.HasMentor is not null)
        {
            conditions.Add(filter.HasMentor.Value ? "s.mentor_user_id IS NOT NULL" : "s.mentor_user_id IS NULL");
        }

        string whereSql = conditions.Count == 0 ? string.Empty : $"WHERE {string.Join(" AND ", conditions)}";

        var dataSql =
            $"""
            SELECT
                s.id AS {nameof(StudentSummaryResponse.Id)},
                s.user_id AS {nameof(StudentSummaryResponse.UserId)},
                s.user_name AS {nameof(StudentSummaryResponse.UserName)},
                s.first_name AS {nameof(StudentSummaryResponse.FirstName)},
                s.last_name AS {nameof(StudentSummaryResponse.LastName)},
                s.school AS {nameof(StudentSummaryResponse.School)},
                s.grade AS {nameof(StudentSummaryResponse.Grade)},
                s.region AS {nameof(StudentSummaryResponse.Region)},
                s.mentor_user_id AS {nameof(StudentSummaryResponse.MentorUserId)},
                s.created_on_utc AS {nameof(StudentSummaryResponse.CreatedOnUtc)}
            FROM student.students s
            {whereSql}
            ORDER BY s.{paging.SortBy} {paging.SortDirection}
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        var countSql = $"SELECT COUNT(*) FROM student.students s {whereSql}";

        var param = new
        {
            Search = filter.SearchPattern,
            Region = (int?)filter.Region,
            filter.Grade,
            Exam = (int?)filter.Exam,
            filter.MentorUserId,
            Offset = paging.First,
            PageSize = paging.Rows
        };

        var items = (await connection.QueryAsync<StudentSummaryResponse>(
            new CommandDefinition(dataSql, param, cancellationToken: cancellationToken))).AsList();

        var totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, param, cancellationToken: cancellationToken));

        return new PagedList<StudentSummaryResponse>(items, totalCount);
    }
}
