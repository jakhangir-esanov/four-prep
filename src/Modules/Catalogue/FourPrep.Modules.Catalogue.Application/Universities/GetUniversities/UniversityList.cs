namespace FourPrep.Modules.Catalogue.Application.Universities.GetUniversities;

using Dapper;
using System.Data.Common;
using FourPrep.Common.Domain.Enums;
using FourPrep.Common.Application.Paging;

internal static class UniversityList
{
    private const string FromSql =
        """
        FROM catalogue.universities u
        LEFT JOIN catalogue.destinations d ON d.country_code = u.country_code
        """;

    public static async Task<PagedList<UniversitySummaryResponse>> ReadAsync(
        DbConnection connection,
        PagingRequest<UniversitySummaryResponse> paging,
        UniversityFilter filter,
        DateOnly staleBefore,
        CancellationToken cancellationToken)
    {
        var conditions = UniversityConditions(filter);

        string whereSql = conditions.Count == 0 ? string.Empty : $"WHERE {string.Join(" AND ", conditions)}";

        var dataSql =
            $"""
            WITH listed AS (
                SELECT
                    u.id,
                    u.slug,
                    u.name,
                    u.country_code,
                    u.city,
                    u.ranking_source,
                    u.ranking,
                    u.acceptance_rate,
                    u.is_partner,
                    d.id IS NOT NULL AS is_service_destination,
                    u.checked_on,
                    u.checked_on < @StaleBefore AS is_stale
                {FromSql}
                {whereSql}
            )
            SELECT
                id AS {nameof(UniversitySummaryResponse.Id)},
                slug AS {nameof(UniversitySummaryResponse.Slug)},
                name AS {nameof(UniversitySummaryResponse.Name)},
                country_code AS {nameof(UniversitySummaryResponse.CountryCode)},
                city AS {nameof(UniversitySummaryResponse.City)},
                ranking_source AS {nameof(UniversitySummaryResponse.RankingSource)},
                ranking AS {nameof(UniversitySummaryResponse.Ranking)},
                acceptance_rate AS {nameof(UniversitySummaryResponse.AcceptanceRate)},
                is_partner AS {nameof(UniversitySummaryResponse.IsPartner)},
                is_service_destination AS {nameof(UniversitySummaryResponse.IsServiceDestination)},
                checked_on AS {nameof(UniversitySummaryResponse.CheckedOn)},
                is_stale AS {nameof(UniversitySummaryResponse.IsStale)}
            FROM listed
            ORDER BY {paging.SortBy} {paging.SortDirection} NULLS LAST, id
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        var countSql = $"SELECT COUNT(*) {FromSql} {whereSql}";

        var param = new
        {
            Search = filter.SearchPattern,
            Country = filter.CountryCode,
            filter.IsPartner,
            DegreeLevel = (int?)filter.DegreeLevel,
            Major = filter.MajorPattern,
            filter.Sat,
            SatRequired = (int)SatPolicy.Required,
            filter.Budget,
            Currency = filter.BudgetCurrency,
            StaleBefore = staleBefore,
            Offset = paging.First,
            PageSize = paging.Rows
        };

        var items = (await connection.QueryAsync<UniversitySummaryResponse>(
            new CommandDefinition(dataSql, param, cancellationToken: cancellationToken))).AsList();

        var totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, param, cancellationToken: cancellationToken));

        return new PagedList<UniversitySummaryResponse>(items, totalCount);
    }

    private static List<string> UniversityConditions(UniversityFilter filter)
    {
        var conditions = new List<string>();

        if (filter.SearchPattern is not null)
        {
            conditions.Add("(u.name ILIKE @Search OR u.city ILIKE @Search)");
        }

        if (filter.CountryCode is not null)
        {
            conditions.Add("u.country_code = @Country");
        }

        if (filter.IsPartner is not null)
        {
            conditions.Add("u.is_partner = @IsPartner");
        }

        if (filter.ServiceDestinationsOnly is true)
        {
            conditions.Add("d.id IS NOT NULL");
        }

        if (filter.BudgetCurrency is not null)
        {
            conditions.Add(
                """
                EXISTS (
                    SELECT 1 FROM catalogue.costs c
                    WHERE c.university_id = u.id
                      AND c.currency = @Currency
                      AND c.tuition_minor + COALESCE(c.living_minor, 0) <= @Budget
                      AND c.academic_year = (SELECT MAX(latest.academic_year) FROM catalogue.costs latest WHERE latest.university_id = u.id))
                """);
        }

        if (filter.HasProgramConditions)
        {
            conditions.Add(
                $"EXISTS (SELECT 1 FROM catalogue.programs p WHERE p.university_id = u.id AND {string.Join(" AND ", ProgramConditions(filter))})");
        }

        return conditions;
    }

    private static List<string> ProgramConditions(UniversityFilter filter)
    {
        var conditions = new List<string>();

        if (filter.DegreeLevel is not null)
        {
            conditions.Add("p.degree_level = @DegreeLevel");
        }

        if (filter.MajorPattern is not null)
        {
            conditions.Add("p.field ILIKE @Major");
        }

        if (filter.Sat is not null)
        {
            conditions.Add(
                """
                EXISTS (
                    SELECT 1 FROM catalogue.admission_requirements r
                    WHERE r.program_id = p.id
                      AND (r.sat_policy IS DISTINCT FROM @SatRequired OR r.sat_mid_range_low IS NULL OR r.sat_mid_range_low <= @Sat))
                """);
        }

        return conditions;
    }
}
