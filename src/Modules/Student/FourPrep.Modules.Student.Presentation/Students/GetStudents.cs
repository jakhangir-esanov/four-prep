namespace FourPrep.Modules.Student.Presentation.Students;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using FourPrep.Common.Application.Paging;
using FourPrep.Common.Domain.Authorization;
using FourPrep.Common.Presentation.Endpoints;
using FourPrep.Modules.Student.Presentation.Students.Requests;
using FourPrep.Modules.Student.Application.Students.GetStudents;

internal sealed class GetStudents : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("students", async (
            [FromServices] ISender sender,
            [AsParameters] GetStudentsRequest request) =>
        {
            var query = new GetStudentsQuery(
                new PagingRequest<StudentSummaryResponse>(
                    request.First ?? 0,
                    request.Rows ?? 10,
                    request.SortField ?? "created_on_utc",
                    request.SortOrder ?? -1),
                new StudentFilter(
                    request.Search,
                    request.Country,
                    request.Grade,
                    request.Exam,
                    request.MentorUserId,
                    request.HasMentor));

            var result = await sender.Send(query);

            return result;
        })
        .RequireAuthorization(Policies.MentorAdmin)
        .Produces<PagedList<StudentSummaryResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithTags("Student / Admin");
    }
}
