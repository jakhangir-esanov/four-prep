namespace FourPrep.Modules.Meeting.Application.StaffSchedules.GetMySchedule;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Meeting.Domain.StaffSchedules;
using FourPrep.Modules.Meeting.Application.Abstractions.Authentication;

public sealed record GetMyScheduleQuery : IQuery<StaffScheduleResponse>;

internal sealed class GetMyScheduleQueryHandler(
    ICurrentUser currentUser,
    IDbConnectionFactory dbConnectionFactory) : IQueryHandler<GetMyScheduleQuery, StaffScheduleResponse>
{
    public async Task<Result<StaffScheduleResponse>> Handle(GetMyScheduleQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        var schedule = await StaffScheduleReader.ReadByStaffAsync(connection, currentUser.UserId, cancellationToken);

        return schedule is null
            ? Result.Failure<StaffScheduleResponse>(StaffScheduleErrors.NotFound)
            : schedule;
    }
}
