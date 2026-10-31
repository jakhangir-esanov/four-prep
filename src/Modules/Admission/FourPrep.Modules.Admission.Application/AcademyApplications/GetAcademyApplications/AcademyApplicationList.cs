namespace FourPrep.Modules.Admission.Application.AcademyApplications.GetAcademyApplications;

using Dapper;
using System.Data.Common;
using FourPrep.Common.Application.Paging;

internal static class AcademyApplicationList
{
    private const string DetailSql =
        $"""
        SELECT
            a.id AS {nameof(AcademyApplicationResponse.Id)},
            a.student_user_id AS {nameof(AcademyApplicationResponse.StudentUserId)},
            a.student_name AS {nameof(AcademyApplicationResponse.StudentName)},
            a.status AS {nameof(AcademyApplicationResponse.Status)},
            a.interests AS {nameof(AcademyApplicationResponse.Interests)},
            a.contact_channels AS {nameof(AcademyApplicationResponse.ContactChannels)},
            a.phone_number AS {nameof(AcademyApplicationResponse.PhoneNumber)},
            a.telegram_username AS {nameof(AcademyApplicationResponse.TelegramUsername)},
            a.meeting_language AS {nameof(AcademyApplicationResponse.MeetingLanguage)},
            a.question AS {nameof(AcademyApplicationResponse.Question)},
            a.parent_consent_confirmed AS {nameof(AcademyApplicationResponse.ParentConsentConfirmed)},
            a.submitted_on_utc AS {nameof(AcademyApplicationResponse.SubmittedOnUtc)},
            a.reviewed_by_user_id AS {nameof(AcademyApplicationResponse.ReviewedByUserId)},
            a.reviewed_on_utc AS {nameof(AcademyApplicationResponse.ReviewedOnUtc)},
            a.decline_reason AS {nameof(AcademyApplicationResponse.DeclineReason)},
            a.withdrawn_on_utc AS {nameof(AcademyApplicationResponse.WithdrawnOnUtc)}
        FROM admission.academy_applications a
        """;

    private const string SummarySql =
        $"""
        SELECT
            a.id AS {nameof(AcademyApplicationSummaryResponse.Id)},
            a.student_user_id AS {nameof(AcademyApplicationSummaryResponse.StudentUserId)},
            a.student_name AS {nameof(AcademyApplicationSummaryResponse.StudentName)},
            a.status AS {nameof(AcademyApplicationSummaryResponse.Status)},
            a.interests AS {nameof(AcademyApplicationSummaryResponse.Interests)},
            a.meeting_language AS {nameof(AcademyApplicationSummaryResponse.MeetingLanguage)},
            a.submitted_on_utc AS {nameof(AcademyApplicationSummaryResponse.SubmittedOnUtc)},
            a.reviewed_on_utc AS {nameof(AcademyApplicationSummaryResponse.ReviewedOnUtc)}
        FROM admission.academy_applications a
        """;

    private const string ScoresSql =
        $"""
        SELECT
            s.academy_application_id AS {nameof(ReportedScoreRow.AcademyApplicationId)},
            s.exam AS {nameof(ReportedScoreRow.Exam)},
            s.score AS {nameof(ReportedScoreRow.Score)}
        FROM admission.reported_scores s
        WHERE s.academy_application_id = ANY(@Ids)
        ORDER BY s.exam
        """;

    public static async Task<AcademyApplicationResponse?> ReadAsync(
        DbConnection connection,
        Guid id,
        CancellationToken cancellationToken)
    {
        const string sql = $"{DetailSql} WHERE a.id = @Id";

        var application = await connection.QuerySingleOrDefaultAsync<AcademyApplicationResponse>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));

        if (application is null)
        {
            return null;
        }

        return (await WithScoresAsync(connection, [application], cancellationToken))[0];
    }

    public static async Task<IReadOnlyList<AcademyApplicationResponse>> ReadForStudentAsync(
        DbConnection connection,
        Guid studentUserId,
        CancellationToken cancellationToken)
    {
        const string sql = $"{DetailSql} WHERE a.student_user_id = @StudentUserId ORDER BY a.submitted_on_utc DESC";

        var applications = await connection.QueryAsync<AcademyApplicationResponse>(
            new CommandDefinition(sql, new { StudentUserId = studentUserId }, cancellationToken: cancellationToken));

        return await WithScoresAsync(connection, applications.AsList(), cancellationToken);
    }

    public static async Task<PagedList<AcademyApplicationSummaryResponse>> ReadPageAsync(
        DbConnection connection,
        PagingRequest<AcademyApplicationSummaryResponse> paging,
        AcademyApplicationFilter filter,
        CancellationToken cancellationToken)
    {
        var conditions = Conditions(filter);
        string whereSql = conditions.Count == 0 ? string.Empty : $"WHERE {string.Join(" AND ", conditions)}";

        var dataSql =
            $"""
            {SummarySql}
            {whereSql}
            ORDER BY a.{paging.SortBy} {paging.SortDirection} NULLS LAST, a.id
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        var countSql = $"SELECT COUNT(*) FROM admission.academy_applications a {whereSql}";

        var param = new
        {
            Status = (int?)filter.Status,
            Interest = (int?)filter.Interest,
            filter.StudentUserId,
            Search = filter.SearchPattern,
            Offset = paging.First,
            PageSize = paging.Rows
        };

        var applications = await connection.QueryAsync<AcademyApplicationSummaryResponse>(
            new CommandDefinition(dataSql, param, cancellationToken: cancellationToken));

        var totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, param, cancellationToken: cancellationToken));

        return new PagedList<AcademyApplicationSummaryResponse>(applications.AsList(), totalCount);
    }

    private static async Task<IReadOnlyList<AcademyApplicationResponse>> WithScoresAsync(
        DbConnection connection,
        List<AcademyApplicationResponse> applications,
        CancellationToken cancellationToken)
    {
        if (applications.Count == 0)
        {
            return applications;
        }

        var ids = applications.Select(application => application.Id).ToArray();

        var scores = (await connection.QueryAsync<ReportedScoreRow>(
                new CommandDefinition(ScoresSql, new { Ids = ids }, cancellationToken: cancellationToken)))
            .ToLookup(score => score.AcademyApplicationId);

        return
        [
            .. applications.Select(application => application with
            {
                Scores = [.. scores[application.Id].Select(score => score.ToResponse())]
            })
        ];
    }

    private static List<string> Conditions(AcademyApplicationFilter filter)
    {
        var conditions = new List<string>();

        if (filter.Status is not null)
        {
            conditions.Add("a.status = @Status");
        }

        if (filter.Interest is not null)
        {
            conditions.Add("a.interests @> jsonb_build_array(@Interest)");
        }

        if (filter.StudentUserId is not null)
        {
            conditions.Add("a.student_user_id = @StudentUserId");
        }

        if (filter.SearchPattern is not null)
        {
            conditions.Add("a.student_name ILIKE @Search");
        }

        return conditions;
    }
}
