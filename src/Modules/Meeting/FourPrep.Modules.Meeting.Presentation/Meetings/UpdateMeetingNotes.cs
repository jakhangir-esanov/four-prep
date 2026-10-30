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
using FourPrep.Modules.Meeting.Application.Meetings.UpdateMeetingNotes;

internal sealed class UpdateMeetingNotes : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("meetings/{id:guid}/notes", async (
            [FromServices] ISender sender,
            Guid id,
            [FromBody] UpdateMeetingNotesRequest request) =>
        {
            var result = await sender.Send(new UpdateMeetingNotesCommand(id, request.Notes));

            return result.ToResponse();
        })
        .RequireAuthorization(Policies.Mentor)
        .Produces<Result>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces<Result>(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Meeting / Staff");
    }
}
