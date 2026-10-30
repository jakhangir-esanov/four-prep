namespace FourPrep.Modules.Content.Application.NewsPosts.GetNewsPosts;

using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Paging;
using FourPrep.Common.Application.Messaging;

public sealed record GetNewsPostsQuery(
    PagingRequest<NewsPostSummaryResponse> Paging,
    NewsPostFilter? Filter = null) : IPagedListQuery<NewsPostSummaryResponse>;

internal sealed class GetNewsPostsQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IPagedListQueryHandler<GetNewsPostsQuery, NewsPostSummaryResponse>
{
    public async Task<PagedList<NewsPostSummaryResponse>> Handle(
        GetNewsPostsQuery request,
        CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        return await NewsPostList.ReadAsync(
            connection,
            request.Paging,
            request.Filter ?? NewsPostFilter.None,
            cancellationToken);
    }
}
