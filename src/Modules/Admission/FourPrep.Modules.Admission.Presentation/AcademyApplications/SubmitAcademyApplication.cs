namespace FourPrep.Modules.Admission.Presentation.AcademyApplications;

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
using FourPrep.Modules.Admission.Presentation.AcademyApplications.Requests;
using FourPrep.Modules.Admission.Application.AcademyApplications.SubmitAcademyApplication;

internal sealed class SubmitAcademyApplication : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("academy-applications", async (
            [FromServices] ISender sender,
            [FromBody] SubmitAcademyApplicationRequest request) =>
        {
            var result = await sender.Send(new SubmitAcademyApplicationCommand(
                request.Interests,
                request.Scores,
                request.ContactChannels,
                request.PhoneNumber,
                request.TelegramUsername,
                request.MeetingLanguage,
                request.Question,
                request.ParentConsentConfirmed));

            return result.ToResponse();
        })
        .RequireAuthorization(Policies.Student)
        .RequireRateLimiting(RateLimitPolicies.Booking)
        .Produces<Result<Guid>>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces<Result>(StatusCodes.Status404NotFound)
        .Produces<Result>(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status429TooManyRequests)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Admission / Family");
    }
}
