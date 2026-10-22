namespace FourPrep.Modules.Auth.Application.UserSessions.GetUserSession;

using Dapper;
using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Paging;
using FourPrep.Common.Application.Messaging;

public sealed record GetUserSessionsQuery(
    PagingRequest<UserSessionResponse> Paging,
    UserSessionFilter? Filter = null) : IPagedListQuery<UserSessionResponse>;

internal sealed class GetUserSessionsQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IPagedListQueryHandler<GetUserSessionsQuery, UserSessionResponse>
{
    public async Task<PagedList<UserSessionResponse>> Handle(GetUserSessionsQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        var paging = request.Paging;
        var filter = request.Filter ?? UserSessionFilter.None;

        var conditions = new List<string>();

        if (filter.UserId is not null)
        {
            conditions.Add("user_id = @UserId");
        }

        string whereSql = conditions.Count == 0 ? string.Empty : $"WHERE {string.Join(" AND ", conditions)}";

        var dataSql =
            $"""
            SELECT
                id AS {nameof(UserSessionResponse.Id)},
                user_id AS {nameof(UserSessionResponse.UserId)},
                access_token_hash AS {nameof(UserSessionResponse.AccessTokenHash)},
                expires_on_utc AS {nameof(UserSessionResponse.ExpiresOnUtc)},
                is_revoked AS {nameof(UserSessionResponse.IsRevoked)},
                created_on_utc AS {nameof(UserSessionResponse.CreatedOnUtc)},
                revoked_on_utc AS {nameof(UserSessionResponse.RevokedOnUtc)}
            FROM
                auth.user_sessions
            {whereSql}
            ORDER BY {paging.SortBy} {paging.SortDirection}
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        var countSql = $"SELECT COUNT(*) FROM auth.user_sessions {whereSql}";

        var param = new
        {
            Offset = paging.First,
            PageSize = paging.Rows,
            filter.UserId
        };

        var items = (await connection.QueryAsync<UserSessionResponse>(dataSql, param)).ToList();
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, param);

        return new PagedList<UserSessionResponse>(items, totalCount);
    }
}
