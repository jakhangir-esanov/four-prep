namespace FourPrep.Modules.Academy.Application.ClassGroups.GetClassGroups;

using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Paging;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Academy.Domain.ClassGroups;

public sealed record GetOpenClassGroupsQuery(
    PagingRequest<ClassGroupResponse> Paging,
    ClassGroupFilter? Filter = null) : IPagedListQuery<ClassGroupResponse>;

internal sealed class GetOpenClassGroupsQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IPagedListQueryHandler<GetOpenClassGroupsQuery, ClassGroupResponse>
{
    public async Task<PagedList<ClassGroupResponse>> Handle(
        GetOpenClassGroupsQuery request,
        CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        var filter = (request.Filter ?? ClassGroupFilter.None) with { Status = ClassGroupStatus.Open };

        var page = await ClassGroupList.ReadAsync(connection, request.Paging, filter, cancellationToken);

        return new PagedList<ClassGroupResponse>([.. page.Data.Select(group => group.ForPublic())], page.TotalCount);
    }
}
