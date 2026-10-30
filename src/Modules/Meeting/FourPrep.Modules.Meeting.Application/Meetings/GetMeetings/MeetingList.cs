namespace FourPrep.Modules.Meeting.Application.Meetings.GetMeetings;

using Dapper;
using System.Data.Common;
using FourPrep.Common.Application.Paging;

internal static class MeetingList
{
    private const string ListedSql = "SELECT m.*, FALSE AS booked_by_me FROM meeting.meetings m";

    private const string SelectSql =
        $"""
        SELECT
            m.id AS {nameof(MeetingResponse.Id)},
            m.kind AS {nameof(MeetingResponse.Kind)},
            m.mode AS {nameof(MeetingResponse.Mode)},
            m.status AS {nameof(MeetingResponse.Status)},
            m.student_user_id AS {nameof(MeetingResponse.StudentUserId)},
            m.student_name AS {nameof(MeetingResponse.StudentName)},
            m.student_phone AS {nameof(MeetingResponse.StudentPhone)},
            m.booked_by_user_id AS {nameof(MeetingResponse.BookedByUserId)},
            m.staff_user_id AS {nameof(MeetingResponse.StaffUserId)},
            m.staff_name AS {nameof(MeetingResponse.StaffName)},
            m.starts_on_utc AS {nameof(MeetingResponse.StartsOnUtc)},
            m.duration_minutes AS {nameof(MeetingResponse.DurationMinutes)},
            m.meeting_url AS {nameof(MeetingResponse.MeetingUrl)},
            m.office_country_code AS {nameof(MeetingResponse.OfficeCountryCode)},
            m.office_city AS {nameof(MeetingResponse.OfficeCity)},
            m.office_address AS {nameof(MeetingResponse.OfficeAddress)},
            m.staff_notes AS {nameof(MeetingResponse.StaffNotes)},
            m.cancellation_reason AS {nameof(MeetingResponse.CancellationReason)},
            m.cancelled_on_utc AS {nameof(MeetingResponse.CancelledOnUtc)},
            m.created_on_utc AS {nameof(MeetingResponse.CreatedOnUtc)}
        FROM listed m
        """;

    public static async Task<PagedList<MeetingResponse>> ReadAsync(
        DbConnection connection,
        PagingRequest<MeetingResponse> paging,
        MeetingFilter filter,
        CancellationToken cancellationToken)
    {
        var conditions = Conditions(filter);
        string whereSql = conditions.Count == 0 ? string.Empty : $"WHERE {string.Join(" AND ", conditions)}";

        var dataSql =
            $"""
            WITH listed AS ({ListedSql} {whereSql})
            {SelectSql}
            ORDER BY m.{paging.SortBy} {paging.SortDirection} NULLS LAST, m.id
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        var countSql = $"SELECT COUNT(*) FROM meeting.meetings m {whereSql}";

        var param = Parameters(filter, paging.First, paging.Rows);

        var meetings = await connection.QueryAsync<MeetingResponse>(
            new CommandDefinition(dataSql, param, cancellationToken: cancellationToken));

        var totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, param, cancellationToken: cancellationToken));

        return new PagedList<MeetingResponse>(meetings.AsList(), totalCount);
    }

    public static async Task<IReadOnlyList<MeetingResponse>> ReadForStudentAsync(
        DbConnection connection,
        Guid studentUserId,
        CancellationToken cancellationToken)
    {
        const string sql =
            $"""
            WITH listed AS ({ListedSql} WHERE m.student_user_id = @StudentUserId)
            {SelectSql}
            ORDER BY m.starts_on_utc DESC
            """;

        return (await connection.QueryAsync<MeetingResponse>(
            new CommandDefinition(sql, new { StudentUserId = studentUserId }, cancellationToken: cancellationToken))).AsList();
    }

    private static object Parameters(MeetingFilter filter, int offset, int pageSize) =>
        new
        {
            Status = (int?)filter.Status,
            Mode = (int?)filter.Mode,
            filter.StaffUserId,
            filter.StudentUserId,
            FromUtc = UtcTimes.From(filter.FromUtc),
            ToUtc = UtcTimes.From(filter.ToUtc),
            Search = filter.SearchPattern,
            Offset = offset,
            PageSize = pageSize
        };

    private static List<string> Conditions(MeetingFilter filter)
    {
        var conditions = new List<string>();

        if (filter.Status is not null)
        {
            conditions.Add("m.status = @Status");
        }

        if (filter.Mode is not null)
        {
            conditions.Add("m.mode = @Mode");
        }

        if (filter.StaffUserId is not null)
        {
            conditions.Add("m.staff_user_id = @StaffUserId");
        }

        if (filter.StudentUserId is not null)
        {
            conditions.Add("m.student_user_id = @StudentUserId");
        }

        if (filter.FromUtc is not null)
        {
            conditions.Add("m.starts_on_utc >= @FromUtc");
        }

        if (filter.ToUtc is not null)
        {
            conditions.Add("m.starts_on_utc < @ToUtc");
        }

        if (filter.SearchPattern is not null)
        {
            conditions.Add("(m.student_name ILIKE @Search OR m.staff_name ILIKE @Search)");
        }

        return conditions;
    }
}
