namespace FourPrep.Modules.Academy.Application.ClassGroups.GetClassGroups;

using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Paging;
using FourPrep.Common.Application.Messaging;

public sealed record GetClassGroupsQuery(
    PagingRequest<ClassGroupResponse> Paging,
    ClassGroupFilter? Filter = null) : IPagedListQuery<ClassGroupResponse>;

internal sealed class GetClassGroupsQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IPagedListQueryHandler<GetClassGroupsQuery, ClassGroupResponse>
{
    public async Task<PagedList<ClassGroupResponse>> Handle(
        GetClassGroupsQuery request,
        CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        return await ClassGroupList.ReadAsync(
            connection,
            request.Paging,
            request.Filter ?? ClassGroupFilter.None,
            cancellationToken);
    }
}
