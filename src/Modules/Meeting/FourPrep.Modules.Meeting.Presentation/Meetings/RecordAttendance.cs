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
using FourPrep.Modules.Meeting.Application.Meetings.RecordAttendance;

internal sealed class RecordAttendance : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("meetings/{id:guid}/attendance", async (
            [FromServices] ISender sender,
            Guid id,
            [FromBody] RecordAttendanceRequest request) =>
        {
            var result = await sender.Send(new RecordAttendanceCommand(id, request.Status));

            return result.ToResponse();
        })
        .RequireAuthorization(Policies.Mentor)
        .Produces<Result>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces<Result>(StatusCodes.Status404NotFound)
        .Produces<Result>(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Meeting / Staff");
    }
}
