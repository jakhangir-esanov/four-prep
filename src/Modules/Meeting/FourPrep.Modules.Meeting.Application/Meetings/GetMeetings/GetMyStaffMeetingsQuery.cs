namespace FourPrep.Modules.Meeting.Application.Meetings.GetMeetings;

using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Paging;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Meeting.Application.Abstractions.Authentication;

public sealed record GetMyStaffMeetingsQuery(
    PagingRequest<MeetingResponse> Paging,
    MeetingFilter? Filter = null) : IPagedListQuery<MeetingResponse>;

internal sealed class GetMyStaffMeetingsQueryHandler(
    ICurrentUser currentUser,
    IDbConnectionFactory dbConnectionFactory) : IPagedListQueryHandler<GetMyStaffMeetingsQuery, MeetingResponse>
{
    public async Task<PagedList<MeetingResponse>> Handle(GetMyStaffMeetingsQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        var filter = (request.Filter ?? MeetingFilter.None) with { StaffUserId = currentUser.UserId };

        var page = await MeetingList.ReadAsync(connection, request.Paging, filter, cancellationToken);

        return new PagedList<MeetingResponse>(
            [.. page.Data.Select(meeting => meeting.ForStaff(currentUser.UserId))],
            page.TotalCount);
    }
}
