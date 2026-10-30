namespace FourPrep.Modules.Meeting.Presentation.Meetings;

using MediatR;
using FourPrep.Common.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Presentation.Results;
using FourPrep.Common.Domain.Authorization;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Modules.Meeting.Presentation.Meetings.Requests;
using FourPrep.Modules.Meeting.Application.Meetings.StaffCancelMeeting;

internal sealed class CancelMeetingAsAdmin : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("admin/meetings/{id:guid}/cancel", async (
            [FromServices] ISender sender,
            Guid id,
            [FromBody] CancelMeetingRequest request) =>
        {
            var result = await sender.Send(new StaffCancelMeetingCommand(id, request.Reason, AnyStaff: true));

            return result.ToResponse();
        })
        .RequireAuthorization(Policies.MentorAdmin)
        .Produces<Result>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces<Result>(StatusCodes.Status404NotFound)
        .Produces<Result>(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Meeting / Admin");
    }
}
