namespace FourPrep.Modules.Meeting.Application.Meetings.GetMeetings;

using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Paging;
using FourPrep.Common.Application.Messaging;

public sealed record GetMeetingsQuery(
    PagingRequest<MeetingResponse> Paging,
    MeetingFilter? Filter = null) : IPagedListQuery<MeetingResponse>;

internal sealed class GetMeetingsQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IPagedListQueryHandler<GetMeetingsQuery, MeetingResponse>
{
    public async Task<PagedList<MeetingResponse>> Handle(GetMeetingsQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        return await MeetingList.ReadAsync(
            connection,
            request.Paging,
            request.Filter ?? MeetingFilter.None,
            cancellationToken);
    }
}
