namespace FourPrep.Modules.Meeting.Application.Meetings.GetConsultationSlots;

using Dapper;
using System.Data.Common;
using FourPrep.Common.Domain;
using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Clock;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Meeting.Domain.Meetings;
using FourPrep.Modules.Meeting.Application.Abstractions.Students;
using FourPrep.Modules.Meeting.Application.Abstractions.Authentication;
using FourPrep.Modules.Meeting.Application.StaffSchedules.GetMySchedule;

public sealed record GetConsultationSlotsQuery(
    Guid StudentUserId,
    MeetingMode? Mode = null,
    DateTime? FromUtc = null,
    DateTime? ToUtc = null) : IQuery<IReadOnlyList<ConsultationSlotResponse>>;

internal sealed class GetConsultationSlotsQueryHandler(
    ICurrentUser currentUser,
    IStudentService studentService,
    IDateTimeProvider dateTimeProvider,
    IDbConnectionFactory dbConnectionFactory) : IQueryHandler<GetConsultationSlotsQuery, IReadOnlyList<ConsultationSlotResponse>>
{
    internal static readonly TimeSpan DefaultRange = TimeSpan.FromDays(14);

    private const string BookedSql =
        $"""
        SELECT
            m.staff_user_id AS {nameof(BookedTime.StaffUserId)},
            m.starts_on_utc AS {nameof(BookedTime.StartsOnUtc)},
            m.duration_minutes AS {nameof(BookedTime.DurationMinutes)}
        FROM meeting.meetings m
        WHERE m.staff_user_id = ANY(@StaffUserIds)
          AND m.status = @Scheduled
          AND m.starts_on_utc < @ToUtc
          AND m.starts_on_utc + make_interval(mins => m.duration_minutes) > @FromUtc
        """;

    public async Task<Result<IReadOnlyList<ConsultationSlotResponse>>> Handle(
        GetConsultationSlotsQuery request,
        CancellationToken cancellationToken)
    {
        var access = await studentService.HasFamilyAccessAsync(currentUser.UserId, request.StudentUserId, cancellationToken);
        if (access.IsFailure)
        {
            return Result.Failure<IReadOnlyList<ConsultationSlotResponse>>(access.Error);
        }

        if (!access.Data)
        {
            return Result.Failure<IReadOnlyList<ConsultationSlotResponse>>(MeetingErrors.NotYourStudent);
        }

        var student = await studentService.GetStudentAsync(request.StudentUserId, cancellationToken);
        if (student.IsFailure)
        {
            return Result.Failure<IReadOnlyList<ConsultationSlotResponse>>(student.Error);
        }

        var utcNow = dateTimeProvider.UtcNow;
        var latestUtc = utcNow + ConsultationSlots.MaxAhead;
        var fromUtc = Earlier(Later(UtcTimes.From(request.FromUtc) ?? utcNow, utcNow + ConsultationSlots.MinNotice), latestUtc);
        var toUtc = Earlier(UtcTimes.From(request.ToUtc) ?? fromUtc + DefaultRange, latestUtc);
        if (toUtc <= fromUtc)
        {
            return Result.Success<IReadOnlyList<ConsultationSlotResponse>>([]);
        }

        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        var schedules = await StaffScheduleReader.ReadBookableAsync(connection, student.Data.MentorUserId, cancellationToken);
        var booked = await ReadBookedAsync(connection, schedules, fromUtc, toUtc, cancellationToken);

        return Result.Success(FreeSlots(schedules, booked, request.Mode, fromUtc, toUtc));
    }

    internal static IReadOnlyList<ConsultationSlotResponse> FreeSlots(
        IReadOnlyList<StaffScheduleResponse> schedules,
        ILookup<Guid, BookedTime> booked,
        MeetingMode? mode,
        DateTime fromUtc,
        DateTime toUtc) =>
        [
            .. schedules
                .SelectMany(schedule => ConsultationSlots
                    .Offered(schedule.TimeZone, Windows(schedule), fromUtc, toUtc)
                    .Where(offered => mode is null || offered.Mode == mode)
                    .Where(offered => !booked[schedule.StaffUserId].Any(time => time.Overlaps(offered.StartsOnUtc)))
                    .GroupBy(offered => offered.StartsOnUtc)
                    .Select(group => new ConsultationSlotResponse(
                        schedule.StaffUserId,
                        schedule.DisplayName,
                        group.Key,
                        Meeting.ConsultationMinutes,
                        [.. group.Select(offered => offered.Mode).Distinct().Order()])))
                .OrderBy(slot => slot.StartsOnUtc)
                .ThenBy(slot => slot.StaffName, StringComparer.Ordinal)
        ];

    private static IReadOnlyCollection<WeeklyWindow> Windows(StaffScheduleResponse schedule) =>
        [.. schedule.Windows.Select(window => new WeeklyWindow(window.Day, window.StartTime, window.EndTime, window.Mode))];

    private static async Task<ILookup<Guid, BookedTime>> ReadBookedAsync(
        DbConnection connection,
        IReadOnlyList<StaffScheduleResponse> schedules,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken)
    {
        if (schedules.Count == 0)
        {
            return Array.Empty<BookedTime>().ToLookup(time => time.StaffUserId);
        }

        var param = new
        {
            StaffUserIds = schedules.Select(schedule => schedule.StaffUserId).ToArray(),
            Scheduled = (int)MeetingStatus.Scheduled,
            FromUtc = fromUtc,
            ToUtc = toUtc
        };

        return (await connection.QueryAsync<BookedTime>(new CommandDefinition(BookedSql, param, cancellationToken: cancellationToken)))
            .ToLookup(time => time.StaffUserId);
    }

    private static DateTime Later(DateTime first, DateTime second) => first > second ? first : second;

    private static DateTime Earlier(DateTime first, DateTime second) => first < second ? first : second;
}

internal sealed record BookedTime
{
    public Guid StaffUserId { get; init; }
    public DateTime StartsOnUtc { get; init; }
    public int DurationMinutes { get; init; }

    public bool Overlaps(DateTime startsOnUtc) =>
        startsOnUtc < StartsOnUtc.AddMinutes(DurationMinutes) && StartsOnUtc < startsOnUtc + ConsultationSlots.Duration;
}
