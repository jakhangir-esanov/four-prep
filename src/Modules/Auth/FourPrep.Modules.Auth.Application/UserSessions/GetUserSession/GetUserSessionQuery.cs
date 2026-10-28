namespace FourPrep.Modules.Auth.Application.UserSessions.GetUserSession;

using Dapper;
using FourPrep.Common.Domain;
using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Messaging;
using FourPrep.Modules.Auth.Domain.UserSessions;

public sealed record GetUserSessionQuery(Guid Id) : IQuery<UserSessionResponse>;

internal sealed class GetUserSessionQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetUserSessionQuery, UserSessionResponse>
{
    public async Task<Result<UserSessionResponse>> Handle(GetUserSessionQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        var sql =
            $"""
            SELECT
                id AS {nameof(UserSessionResponse.Id)},
                user_id AS {nameof(UserSessionResponse.UserId)},
                access_token_hash AS {nameof(UserSessionResponse.AccessTokenHash)},
                expires_on_utc AS {nameof(UserSessionResponse.ExpiresOnUtc)},
                is_revoked AS {nameof(UserSessionResponse.IsRevoked)},
                created_on_utc AS {nameof(UserSessionResponse.CreatedOnUtc)},
                refreshed_on_utc AS {nameof(UserSessionResponse.RefreshedOnUtc)},
                revoked_on_utc AS {nameof(UserSessionResponse.RevokedOnUtc)}
            FROM
                auth.user_sessions
            WHERE
                id = @Id
            """;

        var userSession = await connection.QuerySingleOrDefaultAsync<UserSessionResponse>(sql, request);
        if (userSession is null)
        {
            return Result.Failure<UserSessionResponse>(UserSessionErrors.NotFound);
        }

        return userSession;
    }
}
