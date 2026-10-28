namespace FourPrep.Modules.Catalogue.Application.Programs.GetMajors;

using Dapper;
using FourPrep.Common.Domain;
using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Messaging;

public sealed record GetMajorsQuery(string? Search) : IQuery<IReadOnlyList<MajorResponse>>;

internal sealed class GetMajorsQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetMajorsQuery, IReadOnlyList<MajorResponse>>
{
    private const int MaxMajors = 20;

    public async Task<Result<IReadOnlyList<MajorResponse>>> Handle(
        GetMajorsQuery request,
        CancellationToken cancellationToken)
    {
        string? searchPattern = LikePatterns.Contains(request.Search);
        string whereSql = searchPattern is null ? string.Empty : "WHERE field ILIKE @Search";

        var sql =
            $"""
            SELECT
                field AS {nameof(MajorResponse.Field)},
                COUNT(*)::int AS {nameof(MajorResponse.ProgramCount)}
            FROM catalogue.programs
            {whereSql}
            GROUP BY field
            ORDER BY COUNT(*) DESC, field
            LIMIT @Limit
            """;

        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        var majors = await connection.QueryAsync<MajorResponse>(
            new CommandDefinition(sql, new { Search = searchPattern, Limit = MaxMajors }, cancellationToken: cancellationToken));

        return majors.AsList();
    }
}
