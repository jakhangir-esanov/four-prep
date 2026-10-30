namespace FourPrep.Modules.Content.Application.NewsPosts.GetNewsPosts;

using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Paging;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Content.Domain.NewsPosts;

public sealed record GetPublishedNewsPostsQuery(
    PagingRequest<NewsPostSummaryResponse> Paging,
    NewsPostFilter? Filter = null) : IPagedListQuery<NewsPostSummaryResponse>;

internal sealed class GetPublishedNewsPostsQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IPagedListQueryHandler<GetPublishedNewsPostsQuery, NewsPostSummaryResponse>
{
    public async Task<PagedList<NewsPostSummaryResponse>> Handle(
        GetPublishedNewsPostsQuery request,
        CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        var filter = (request.Filter ?? NewsPostFilter.None) with
        {
            Status = NewsPostStatus.Published,
            Search = null
        };

        return await NewsPostList.ReadAsync(connection, request.Paging, filter, cancellationToken);
    }
}
