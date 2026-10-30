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
using FourPrep.Common.Presentation.RateLimiting;
using FourPrep.Modules.Meeting.Presentation.Meetings.Requests;
using FourPrep.Modules.Meeting.Application.Meetings.GetConsultationSlots;

internal sealed class GetConsultationSlots : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("meetings/slots", async (
            [FromServices] ISender sender,
            [AsParameters] GetConsultationSlotsRequest request) =>
        {
            var result = await sender.Send(new GetConsultationSlotsQuery(
                request.StudentUserId,
                request.Mode,
                request.FromUtc,
                request.ToUtc));

            return result.ToResponse();
        })
        .RequireAuthorization(Policies.StudentOrParent)
        .RequireRateLimiting(RateLimitPolicies.PublicRead)
        .Produces<Result<IReadOnlyList<ConsultationSlotResponse>>>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces<Result>(StatusCodes.Status403Forbidden)
        .Produces<Result>(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status429TooManyRequests)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Meeting / Family");
    }
}
