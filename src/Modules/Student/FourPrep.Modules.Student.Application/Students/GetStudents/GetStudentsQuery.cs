namespace FourPrep.Modules.Student.Application.Students.GetStudents;

using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Paging;
using FourPrep.Common.Application.Messaging;

public sealed record GetStudentsQuery(
    PagingRequest<StudentSummaryResponse> Paging,
    StudentFilter? Filter = null) : IPagedListQuery<StudentSummaryResponse>;

internal sealed class GetStudentsQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IPagedListQueryHandler<GetStudentsQuery, StudentSummaryResponse>
{
    public async Task<PagedList<StudentSummaryResponse>> Handle(
        GetStudentsQuery request,
        CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        return await StudentList.ReadAsync(
            connection,
            request.Paging,
            request.Filter ?? StudentFilter.None,
            cancellationToken);
    }
}
