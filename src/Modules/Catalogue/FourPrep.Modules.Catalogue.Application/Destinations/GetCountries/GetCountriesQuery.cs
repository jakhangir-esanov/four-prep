namespace FourPrep.Modules.Catalogue.Application.Destinations.GetCountries;

using Dapper;
using FourPrep.Common.Domain;
using FourPrep.Common.Application.Data;
using FourPrep.Common.Application.Messaging;

public sealed record GetCountriesQuery : IQuery<IReadOnlyList<CountryResponse>>;

internal sealed class GetCountriesQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetCountriesQuery, IReadOnlyList<CountryResponse>>
{
    private const string Sql =
        $"""
        SELECT
            COALESCE(u.country_code, d.country_code) AS {nameof(CountryResponse.CountryCode)},
            COALESCE(u.university_count, 0) AS {nameof(CountryResponse.UniversityCount)},
            d.id IS NOT NULL AS {nameof(CountryResponse.IsServiceDestination)},
            d.visa_type AS {nameof(CountryResponse.VisaType)}
        FROM (
            SELECT country_code, COUNT(*)::int AS university_count
            FROM catalogue.universities
            GROUP BY country_code
        ) u
        FULL OUTER JOIN catalogue.destinations d ON d.country_code = u.country_code
        ORDER BY 1
        """;

    public async Task<Result<IReadOnlyList<CountryResponse>>> Handle(
        GetCountriesQuery request,
        CancellationToken cancellationToken)
    {
        await using var connection = await dbConnectionFactory.OpenConnectionAsync();

        var countries = await connection.QueryAsync<CountryResponse>(
            new CommandDefinition(Sql, cancellationToken: cancellationToken));

        return countries.AsList();
    }
}
