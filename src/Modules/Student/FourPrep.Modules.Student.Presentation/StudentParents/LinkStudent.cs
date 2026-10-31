namespace FourPrep.Modules.Student.Presentation.StudentParents;

using MediatR;
using FourPrep.Common.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Domain.Authorization;
using FourPrep.Common.Presentation.Results;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Common.Presentation.RateLimiting;
using FourPrep.Modules.Student.Presentation.StudentParents.Requests;
using FourPrep.Modules.Student.Application.StudentParents.LinkStudent;

internal sealed class LinkStudent : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("parents/me/students", async (
            [FromServices] ISender sender,
            [FromBody] LinkStudentRequest request) =>
        {
            var result = await sender.Send(new LinkStudentCommand(request.LinkCode));

            return result.ToResponse();
        })
        .RequireAuthorization(Policies.Parent)
        .RequireRateLimiting(RateLimitPolicies.LinkCode)
        .Produces<Result<Guid>>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces<Result>(StatusCodes.Status404NotFound)
        .Produces<Result>(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status429TooManyRequests)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Student / Parents");
    }
}
