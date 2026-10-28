namespace FourPrep.Modules.Catalogue.Presentation.Programs;

using MediatR;
using FourPrep.Common.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Domain.Authorization;
using FourPrep.Common.Presentation.Results;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Modules.Catalogue.Presentation.Programs.Requests;
using FourPrep.Modules.Catalogue.Application.Programs.ReplaceProgramRequirements;

internal sealed class ReplaceProgramRequirements : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("admin/programs/{id:guid}/requirements", async (
            [FromServices] ISender sender,
            Guid id,
            [FromBody] ReplaceProgramRequirementsRequest request) =>
        {
            var command = new ReplaceProgramRequirementsCommand(
                id,
                request.Requirements?.Select(requirement => new AdmissionRequirementInput(
                    requirement.IntakeTerm,
                    requirement.IntakeYear,
                    requirement.EnglishMinimums,
                    requirement.SatPolicy,
                    requirement.SatMidRangeLow,
                    requirement.SatMidRangeHigh,
                    requirement.MinimumGpa,
                    requirement.GpaScale,
                    requirement.Documents,
                    requirement.EssaysCount,
                    requirement.InterviewRequired,
                    requirement.SourceUrl ?? string.Empty,
                    requirement.CheckedOn)).ToList() ?? []);

            var result = await sender.Send(command);

            return result.ToResponse();
        })
        .RequireAuthorization(Policies.ContentAdmin)
        .Produces<Result>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces<Result>(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Catalogue / Admin");
    }
}
