namespace FourPrep.Modules.Catalogue.Presentation.Universities;

using MediatR;
using FourPrep.Common.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Domain.Authorization;
using FourPrep.Common.Presentation.Results;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Modules.Catalogue.Presentation.Universities.Requests;
using FourPrep.Modules.Catalogue.Application.Universities.ReplaceUniversityScholarships;

internal sealed class ReplaceUniversityScholarships : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("admin/universities/{id:guid}/scholarships", async (
            [FromServices] ISender sender,
            Guid id,
            [FromBody] ReplaceUniversityScholarshipsRequest request) =>
        {
            var command = new ReplaceUniversityScholarshipsCommand(
                id,
                request.Scholarships?.Select(scholarship => new ScholarshipInput(
                    scholarship.Name ?? string.Empty,
                    scholarship.Kind,
                    scholarship.Amount,
                    scholarship.Eligibility,
                    scholarship.SourceUrl ?? string.Empty,
                    scholarship.CheckedOn)).ToList() ?? []);

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
