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
using FourPrep.Modules.Meeting.Application.Meetings.BookConsultation;

internal sealed class BookConsultation : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("meetings", async (
            [FromServices] ISender sender,
            [FromBody] BookConsultationRequest request) =>
        {
            var result = await sender.Send(new BookConsultationCommand(
                request.StudentUserId,
                request.StaffUserId,
                request.StartsOnUtc,
                request.Mode,
                request.PhoneNumber));

            return result.ToResponse();
        })
        .RequireAuthorization(Policies.StudentOrParent)
        .RequireRateLimiting(RateLimitPolicies.Booking)
        .Produces<Result<Guid>>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces<Result>(StatusCodes.Status403Forbidden)
        .Produces<Result>(StatusCodes.Status404NotFound)
        .Produces<Result>(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status429TooManyRequests)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Meeting / Family");
    }
}
