namespace FourPrep.Modules.Content.Application.NewsPosts.GetNewsPosts;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Content.Domain.NewsPosts;

public sealed record GetPublishedNewsPostQuery(string Slug) : IQuery<NewsPostResponse>;

internal sealed class GetPublishedNewsPostQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetPublishedNewsPostQuery, NewsPostResponse>
{
    public async Task<Result<NewsPostResponse>> Handle(
        GetPublishedNewsPostQuery request,
        CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        var newsPost = await NewsPostList.ReadPublishedAsync(
            connection,
            request.Slug.Trim().ToLowerInvariant(),
            cancellationToken);

        return newsPost is null
            ? Result.Failure<NewsPostResponse>(NewsPostErrors.NotFound)
            : newsPost.ForPublic();
    }
}
