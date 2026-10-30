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
using FourPrep.Modules.Meeting.Application.StaffSchedules.GetMySchedule;

internal sealed class GetMySchedule : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("meetings/schedules/me", async (
            [FromServices] ISender sender) =>
        {
            var result = await sender.Send(new GetMyScheduleQuery());

            return result.ToResponse();
        })
        .RequireAuthorization(Policies.Mentor)
        .Produces<Result<StaffScheduleResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces<Result>(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Meeting / Staff");
    }
}
