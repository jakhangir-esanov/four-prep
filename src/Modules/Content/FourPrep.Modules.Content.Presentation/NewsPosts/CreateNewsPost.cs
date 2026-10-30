namespace FourPrep.Modules.Content.Presentation.NewsPosts;

using MediatR;
using FourPrep.Common.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Presentation.Results;
using FourPrep.Common.Domain.Authorization;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Modules.Content.Presentation.NewsPosts.Requests;
using FourPrep.Modules.Content.Application.NewsPosts.CreateNewsPost;

internal sealed class CreateNewsPost : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("admin/news", async (
            [FromServices] ISender sender,
            [FromBody] NewsPostRequest request) =>
        {
            var result = await sender.Send(new CreateNewsPostCommand(request.ToDetails()));

            return result.ToResponse();
        })
        .RequireAuthorization(Policies.ContentAdmin)
        .Produces<Result<Guid>>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces<Result>(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Content / Admin");
    }
}
