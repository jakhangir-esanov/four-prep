namespace FourPrep.Modules.Academy.Application.ClassGroups.GetClassGroups;

using Dapper;
using System.Data.Common;
using FourPrep.Common.Application.Paging;
using FourPrep.Modules.Academy.Domain.ClassGroups;
using FourPrep.Modules.Academy.Domain.Enrolments;

internal static class ClassGroupList
{
    private const string ListedSql =
        """
        SELECT
            g.id,
            g.course_id,
            c.slug AS course_slug,
            c.name AS course_name,
            g.code,
            g.mode,
            g.country_code,
            g.city,
            g.address,
            g.entry_level,
            g.time_zone,
            g.starts_on,
            g.weeks,
            g.capacity,
            g.capacity - (
                SELECT COUNT(*)::int
                FROM academy.enrolments e
                WHERE e.class_group_id = g.id AND e.status IN (@PendingPayment, @Active)) AS seats_left,
            g.status,
            g.teacher_user_id,
            g.teacher_name,
            g.created_on_utc
        FROM academy.class_groups g
        JOIN academy.courses c ON c.id = g.course_id
        """;

    private const string SelectSql =
        $"""
        SELECT
            id AS {nameof(ClassGroupResponse.Id)},
            course_id AS {nameof(ClassGroupResponse.CourseId)},
            course_slug AS {nameof(ClassGroupResponse.CourseSlug)},
            course_name AS {nameof(ClassGroupResponse.CourseName)},
            code AS {nameof(ClassGroupResponse.Code)},
            mode AS {nameof(ClassGroupResponse.Mode)},
            country_code AS {nameof(ClassGroupResponse.CountryCode)},
            city AS {nameof(ClassGroupResponse.City)},
            address AS {nameof(ClassGroupResponse.Address)},
            entry_level AS {nameof(ClassGroupResponse.EntryLevel)},
            time_zone AS {nameof(ClassGroupResponse.TimeZone)},
            starts_on AS {nameof(ClassGroupResponse.StartsOn)},
            weeks AS {nameof(ClassGroupResponse.Weeks)},
            capacity AS {nameof(ClassGroupResponse.Capacity)},
            seats_left AS {nameof(ClassGroupResponse.SeatsLeft)},
            status AS {nameof(ClassGroupResponse.Status)},
            teacher_user_id AS {nameof(ClassGroupResponse.TeacherUserId)},
            teacher_name AS {nameof(ClassGroupResponse.TeacherName)},
            created_on_utc AS {nameof(ClassGroupResponse.CreatedOnUtc)}
        FROM listed
        """;

    private const string ScheduleSql =
        $"""
        SELECT
            class_group_id AS {nameof(ScheduleRow.ClassGroupId)},
            day AS {nameof(ScheduleRow.Day)},
            start_time AS {nameof(ScheduleRow.StartTime)},
            duration_minutes AS {nameof(ScheduleRow.DurationMinutes)}
        FROM academy.schedule_slots
        WHERE class_group_id = ANY(@Ids)
        ORDER BY day, start_time
        """;

    public static async Task<PagedList<ClassGroupResponse>> ReadAsync(
        DbConnection connection,
        PagingRequest<ClassGroupResponse> paging,
        ClassGroupFilter filter,
        CancellationToken cancellationToken)
    {
        var conditions = Conditions(filter);

        string whereSql = conditions.Count == 0 ? string.Empty : $"WHERE {string.Join(" AND ", conditions)}";

        var dataSql =
            $"""
            WITH listed AS ({ListedSql} {whereSql})
            {SelectSql}
            ORDER BY {paging.SortBy} {paging.SortDirection} NULLS LAST, id
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        var countSql =
            $"""
            SELECT COUNT(*)
            FROM academy.class_groups g
            JOIN academy.courses c ON c.id = g.course_id
            {whereSql}
            """;

        var param = new
        {
            Course = filter.CourseSlug,
            Mode = (int?)filter.Mode,
            Status = (int?)filter.Status,
            filter.TeacherUserId,
            Search = filter.SearchPattern,
            PendingPayment = (int)EnrolmentStatus.PendingPayment,
            Active = (int)EnrolmentStatus.Active,
            Offset = paging.First,
            PageSize = paging.Rows
        };

        var groups = (await connection.QueryAsync<ClassGroupResponse>(
            new CommandDefinition(dataSql, param, cancellationToken: cancellationToken))).AsList();

        var totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, param, cancellationToken: cancellationToken));

        return new PagedList<ClassGroupResponse>(
            await WithScheduleAsync(connection, groups, cancellationToken),
            totalCount);
    }

    public static async Task<ClassGroupResponse?> ReadAsync(
        DbConnection connection,
        Guid id,
        ClassGroupStatus status,
        CancellationToken cancellationToken)
    {
        var sql =
            $"""
            WITH listed AS ({ListedSql} WHERE g.id = @Id AND g.status = @Status)
            {SelectSql}
            """;

        var param = new
        {
            Id = id,
            Status = (int)status,
            PendingPayment = (int)EnrolmentStatus.PendingPayment,
            Active = (int)EnrolmentStatus.Active
        };

        var group = await connection.QuerySingleOrDefaultAsync<ClassGroupResponse>(
            new CommandDefinition(sql, param, cancellationToken: cancellationToken));

        if (group is null)
        {
            return null;
        }

        return (await WithScheduleAsync(connection, [group], cancellationToken))[0];
    }

    private static List<string> Conditions(ClassGroupFilter filter)
    {
        var conditions = new List<string>();

        if (filter.CourseSlug is not null)
        {
            conditions.Add("c.slug = @Course");
        }

        if (filter.Mode is not null)
        {
            conditions.Add("g.mode = @Mode");
        }

        if (filter.Status is not null)
        {
            conditions.Add("g.status = @Status");
        }

        if (filter.TeacherUserId is not null)
        {
            conditions.Add("g.teacher_user_id = @TeacherUserId");
        }

        if (filter.SearchPattern is not null)
        {
            conditions.Add("(g.code ILIKE @Search OR g.teacher_name ILIKE @Search OR g.city ILIKE @Search)");
        }

        return conditions;
    }

    private static async Task<List<ClassGroupResponse>> WithScheduleAsync(
        DbConnection connection,
        List<ClassGroupResponse> groups,
        CancellationToken cancellationToken)
    {
        if (groups.Count == 0)
        {
            return groups;
        }

        var schedule = (await connection.QueryAsync<ScheduleRow>(
                new CommandDefinition(
                    ScheduleSql,
                    new { Ids = groups.Select(group => group.Id).ToArray() },
                    cancellationToken: cancellationToken)))
            .ToLookup(row => row.ClassGroupId);

        return
        [
            .. groups.Select(group => group with
            {
                Schedule = [.. schedule[group.Id].Select(row => row.ToResponse())]
            })
        ];
    }
}
