namespace FourPrep.Modules.Content.Application.NewsPosts.GetNewsPosts;

using FourPrep.Common.Domain;
using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Content.Domain.NewsPosts;

public sealed record GetNewsPostQuery(Guid Id) : IQuery<NewsPostResponse>;

internal sealed class GetNewsPostQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetNewsPostQuery, NewsPostResponse>
{
    public async Task<Result<NewsPostResponse>> Handle(GetNewsPostQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        var newsPost = await NewsPostList.ReadAsync(connection, request.Id, cancellationToken);

        return newsPost is null
            ? Result.Failure<NewsPostResponse>(NewsPostErrors.NotFound)
            : newsPost;
    }
}
