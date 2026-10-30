namespace FourPrep.Modules.Content.Application.NewsPosts.GetNewsPosts;

using Dapper;
using System.Data.Common;
using FourPrep.Common.Application.Paging;
using FourPrep.Modules.Content.Domain.NewsPosts;

internal static class NewsPostList
{
    private const string SummarySql =
        $"""
        SELECT
            p.id AS {nameof(NewsPostSummaryResponse.Id)},
            p.slug AS {nameof(NewsPostSummaryResponse.Slug)},
            p.title AS {nameof(NewsPostSummaryResponse.Title)},
            p.tag AS {nameof(NewsPostSummaryResponse.Tag)},
            p.cover_url AS {nameof(NewsPostSummaryResponse.CoverUrl)},
            p.status AS {nameof(NewsPostSummaryResponse.Status)},
            p.published_on_utc AS {nameof(NewsPostSummaryResponse.PublishedOnUtc)},
            p.created_on_utc AS {nameof(NewsPostSummaryResponse.CreatedOnUtc)},
            p.updated_on_utc AS {nameof(NewsPostSummaryResponse.UpdatedOnUtc)}
        FROM content.news_posts p
        """;

    private const string DetailSql =
        $"""
        SELECT
            p.id AS {nameof(NewsPostResponse.Id)},
            p.slug AS {nameof(NewsPostResponse.Slug)},
            p.title AS {nameof(NewsPostResponse.Title)},
            p.body AS {nameof(NewsPostResponse.Body)},
            p.tag AS {nameof(NewsPostResponse.Tag)},
            p.cover_file_id AS {nameof(NewsPostResponse.CoverFileId)},
            p.cover_url AS {nameof(NewsPostResponse.CoverUrl)},
            p.status AS {nameof(NewsPostResponse.Status)},
            p.published_on_utc AS {nameof(NewsPostResponse.PublishedOnUtc)},
            p.created_on_utc AS {nameof(NewsPostResponse.CreatedOnUtc)},
            p.updated_on_utc AS {nameof(NewsPostResponse.UpdatedOnUtc)}
        FROM content.news_posts p
        """;

    public static async Task<PagedList<NewsPostSummaryResponse>> ReadAsync(
        DbConnection connection,
        PagingRequest<NewsPostSummaryResponse> paging,
        NewsPostFilter filter,
        CancellationToken cancellationToken)
    {
        var conditions = Conditions(filter);

        string whereSql = conditions.Count == 0 ? string.Empty : $"WHERE {string.Join(" AND ", conditions)}";

        var dataSql =
            $"""
            {SummarySql}
            {whereSql}
            ORDER BY p.{paging.SortBy} {paging.SortDirection} NULLS LAST, p.id
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        var countSql = $"SELECT COUNT(*) FROM content.news_posts p {whereSql}";

        var param = new
        {
            Status = (int?)filter.Status,
            Tag = filter.NormalizedTag,
            Locale = filter.NormalizedLocale,
            Search = filter.SearchPattern,
            Offset = paging.First,
            PageSize = paging.Rows
        };

        var newsPosts = await connection.QueryAsync<NewsPostSummaryResponse>(
            new CommandDefinition(dataSql, param, cancellationToken: cancellationToken));

        var totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, param, cancellationToken: cancellationToken));

        return new PagedList<NewsPostSummaryResponse>(newsPosts.AsList(), totalCount);
    }

    public static async Task<NewsPostResponse?> ReadAsync(
        DbConnection connection,
        Guid id,
        CancellationToken cancellationToken)
    {
        const string sql = $"{DetailSql} WHERE p.id = @Id";

        return await connection.QuerySingleOrDefaultAsync<NewsPostResponse>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }

    public static async Task<NewsPostResponse?> ReadPublishedAsync(
        DbConnection connection,
        string slug,
        CancellationToken cancellationToken)
    {
        const string sql = $"{DetailSql} WHERE p.slug = @Slug AND p.status = @Published";

        return await connection.QuerySingleOrDefaultAsync<NewsPostResponse>(
            new CommandDefinition(
                sql,
                new { Slug = slug, Published = (int)NewsPostStatus.Published },
                cancellationToken: cancellationToken));
    }

    private static List<string> Conditions(NewsPostFilter filter)
    {
        var conditions = new List<string>();

        if (filter.Status is not null)
        {
            conditions.Add("p.status = @Status");
        }

        if (filter.NormalizedTag is not null)
        {
            conditions.Add("p.tag = @Tag");
        }

        if (filter.NormalizedLocale is not null)
        {
            conditions.Add("p.title ->> @Locale IS NOT NULL");
        }

        if (filter.SearchPattern is not null)
        {
            conditions.Add(
                "(p.slug ILIKE @Search OR EXISTS (SELECT 1 FROM jsonb_each_text(p.title) t WHERE t.value ILIKE @Search))");
        }

        return conditions;
    }
}
