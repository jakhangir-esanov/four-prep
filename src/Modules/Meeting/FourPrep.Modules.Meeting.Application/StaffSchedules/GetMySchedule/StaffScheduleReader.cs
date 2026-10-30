namespace FourPrep.Modules.Meeting.Application.StaffSchedules.GetMySchedule;

using Dapper;
using System.Data.Common;

internal static class StaffScheduleReader
{
    private const string ScheduleSql =
        $"""
        SELECT
            s.id AS {nameof(StaffScheduleResponse.Id)},
            s.staff_user_id AS {nameof(StaffScheduleResponse.StaffUserId)},
            s.display_name AS {nameof(StaffScheduleResponse.DisplayName)},
            s.time_zone AS {nameof(StaffScheduleResponse.TimeZone)},
            s.meeting_url AS {nameof(StaffScheduleResponse.MeetingUrl)},
            s.office_country_code AS {nameof(StaffScheduleResponse.OfficeCountryCode)},
            s.office_city AS {nameof(StaffScheduleResponse.OfficeCity)},
            s.office_address AS {nameof(StaffScheduleResponse.OfficeAddress)},
            s.updated_on_utc AS {nameof(StaffScheduleResponse.UpdatedOnUtc)}
        FROM meeting.staff_schedules s
        """;

    private const string WindowSql =
        $"""
        SELECT
            w.staff_schedule_id AS {nameof(AvailabilityWindowResponse.StaffScheduleId)},
            w.day AS {nameof(AvailabilityWindowResponse.Day)},
            w.start_time AS {nameof(AvailabilityWindowResponse.StartTime)},
            w.end_time AS {nameof(AvailabilityWindowResponse.EndTime)},
            w.mode AS {nameof(AvailabilityWindowResponse.Mode)}
        FROM meeting.availability_windows w
        WHERE w.staff_schedule_id = ANY(@Ids)
        ORDER BY w.day, w.start_time, w.mode
        """;

    public static async Task<StaffScheduleResponse?> ReadByStaffAsync(
        DbConnection connection,
        Guid staffUserId,
        CancellationToken cancellationToken)
    {
        var schedules = await ReadAsync(connection, "WHERE s.staff_user_id = @StaffUserId", new { StaffUserId = staffUserId }, cancellationToken);

        return schedules.SingleOrDefault();
    }

    public static async Task<IReadOnlyList<StaffScheduleResponse>> ReadBookableAsync(
        DbConnection connection,
        Guid? onlyStaffUserId,
        CancellationToken cancellationToken)
    {
        const string condition =
            """
            WHERE (CAST(@StaffUserId AS uuid) IS NULL OR s.staff_user_id = @StaffUserId)
              AND EXISTS (SELECT 1 FROM meeting.availability_windows w WHERE w.staff_schedule_id = s.id)
            """;

        return await ReadAsync(connection, condition, new { StaffUserId = onlyStaffUserId }, cancellationToken);
    }

    private static async Task<IReadOnlyList<StaffScheduleResponse>> ReadAsync(
        DbConnection connection,
        string condition,
        object param,
        CancellationToken cancellationToken)
    {
        var schedules = (await connection.QueryAsync<StaffScheduleResponse>(
            new CommandDefinition($"{ScheduleSql} {condition}", param, cancellationToken: cancellationToken))).AsList();

        if (schedules.Count == 0)
        {
            return schedules;
        }

        var windows = (await connection.QueryAsync<AvailabilityWindowResponse>(
                new CommandDefinition(
                    WindowSql,
                    new { Ids = schedules.Select(schedule => schedule.Id).ToArray() },
                    cancellationToken: cancellationToken)))
            .ToLookup(window => window.StaffScheduleId);

        return [.. schedules.Select(schedule => schedule with { Windows = [.. windows[schedule.Id]] })];
    }
}
