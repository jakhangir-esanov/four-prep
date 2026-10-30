namespace FourPrep.Modules.Meeting.Presentation.Meetings;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Application.Paging;
using FourPrep.Common.Domain.Authorization;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Modules.Meeting.Presentation.Meetings.Requests;
using FourPrep.Modules.Meeting.Application.Meetings.GetMeetings;

internal sealed class GetMeetings : IEndpoint
{
    private const int DefaultRows = 20;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("admin/meetings", async (
            [FromServices] ISender sender,
            [AsParameters] GetMeetingsRequest request) =>
        {
            var query = new GetMeetingsQuery(
                new PagingRequest<MeetingResponse>(
                    request.First ?? 0,
                    request.Rows ?? DefaultRows,
                    request.SortField ?? "starts_on_utc",
                    request.SortOrder ?? -1),
                request.ToFilter());

            var result = await sender.Send(query);

            return result;
        })
        .RequireAuthorization(Policies.MentorAdmin)
        .Produces<PagedList<MeetingResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Meeting / Admin");
    }
}
