namespace FourPrep.Modules.Meeting.Presentation.StaffSchedules;

using MediatR;
using FourPrep.Common.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Presentation.Results;
using FourPrep.Common.Domain.Authorization;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Modules.Meeting.Presentation.StaffSchedules.Requests;
using FourPrep.Modules.Meeting.Application.StaffSchedules.SaveMySchedule;

internal sealed class SaveMySchedule : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("meetings/schedules/me", async (
            [FromServices] ISender sender,
            [FromBody] ScheduleRequest request) =>
        {
            var result = await sender.Send(new SaveMyScheduleCommand(request.ToDetails()));

            return result.ToResponse();
        })
        .RequireAuthorization(Policies.Mentor)
        .Produces<Result>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces<Result>(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Meeting / Staff");
    }
}
