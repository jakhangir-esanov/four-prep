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
using FourPrep.Modules.Admission.Application.AcademyApplications.WithdrawAcademyApplication;

internal sealed class WithdrawAcademyApplication : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("academy-applications/{id:guid}/withdraw", async (
            [FromServices] ISender sender,
            Guid id) =>
        {
            var result = await sender.Send(new WithdrawAcademyApplicationCommand(id));

            return result.ToResponse();
        })
        .RequireAuthorization(Policies.Student)
        .Produces<Result>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces<Result>(StatusCodes.Status404NotFound)
        .Produces<Result>(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Admission / Family");
    }
}
