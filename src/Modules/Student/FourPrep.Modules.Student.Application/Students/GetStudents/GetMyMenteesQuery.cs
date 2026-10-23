namespace FourPrep.Modules.Student.Application.Students.GetStudents;

using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Paging;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Student.Application.Abstractions.Authentication;

public sealed record GetMyMenteesQuery(
    PagingRequest<StudentSummaryResponse> Paging,
    StudentFilter? Filter = null) : IPagedListQuery<StudentSummaryResponse>;

internal sealed class GetMyMenteesQueryHandler(
    ICurrentUser currentUser,
    IDbConnectionFactory dbConnectionFactory) : IPagedListQueryHandler<GetMyMenteesQuery, StudentSummaryResponse>
{
    public async Task<PagedList<StudentSummaryResponse>> Handle(
        GetMyMenteesQuery request,
        CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        var filter = (request.Filter ?? StudentFilter.None) with
        {
            MentorUserId = currentUser.UserId,
            HasMentor = null
        };

        return await StudentList.ReadAsync(connection, request.Paging, filter, cancellationToken);
    }
}
