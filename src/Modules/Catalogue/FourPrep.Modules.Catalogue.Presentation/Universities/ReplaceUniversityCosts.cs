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
using FourPrep.Modules.Catalogue.Application.Universities.ReplaceUniversityCosts;

internal sealed class ReplaceUniversityCosts : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("admin/universities/{id:guid}/costs", async (
            [FromServices] ISender sender,
            Guid id,
            [FromBody] ReplaceUniversityCostsRequest request) =>
        {
            var command = new ReplaceUniversityCostsCommand(
                id,
                request.Costs?.Select(cost => new CostInput(
                    cost.AcademicYear,
                    cost.TuitionMinor,
                    cost.LivingMinor,
                    cost.Currency ?? string.Empty,
                    cost.SourceUrl ?? string.Empty,
                    cost.CheckedOn)).ToList() ?? []);

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
