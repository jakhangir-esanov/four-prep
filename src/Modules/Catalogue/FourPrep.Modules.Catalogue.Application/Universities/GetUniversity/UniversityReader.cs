namespace FourPrep.Modules.Catalogue.Application.Universities.GetUniversity;

using Dapper;
using System.Data.Common;

internal static class UniversityReader
{
    private const string UniversityIdBySlug = "(SELECT id FROM catalogue.universities WHERE slug = @Slug)";

    private const string ProgramIdsBySlug =
        $"(SELECT id FROM catalogue.programs WHERE university_id = {UniversityIdBySlug})";

    private const string UniversitySql =
        $"""
        SELECT
            u.id AS {nameof(UniversityResponse.Id)},
            u.slug AS {nameof(UniversityResponse.Slug)},
            u.name AS {nameof(UniversityResponse.Name)},
            u.country_code AS {nameof(UniversityResponse.CountryCode)},
            u.city AS {nameof(UniversityResponse.City)},
            u.website AS {nameof(UniversityResponse.Website)},
            u.ranking_source AS {nameof(UniversityResponse.RankingSource)},
            u.ranking AS {nameof(UniversityResponse.Ranking)},
            u.acceptance_rate AS {nameof(UniversityResponse.AcceptanceRate)},
            u.is_partner AS {nameof(UniversityResponse.IsPartner)},
            d.id IS NOT NULL AS {nameof(UniversityResponse.IsServiceDestination)},
            d.visa_type AS {nameof(UniversityResponse.VisaType)},
            u.description AS {nameof(UniversityResponse.Description)},
            u.source_url AS {nameof(UniversityResponse.SourceUrl)},
            u.checked_on AS {nameof(UniversityResponse.CheckedOn)},
            u.checked_on < @StaleBefore AS {nameof(UniversityResponse.IsStale)}
        FROM catalogue.universities u
        LEFT JOIN catalogue.destinations d ON d.country_code = u.country_code
        WHERE u.slug = @Slug;
        """;

    private const string ProgramsSql =
        $"""
        SELECT
            id AS {nameof(ProgramResponse.Id)},
            name AS {nameof(ProgramResponse.Name)},
            degree_level AS {nameof(ProgramResponse.DegreeLevel)},
            field AS {nameof(ProgramResponse.Field)},
            language AS {nameof(ProgramResponse.Language)},
            duration_months AS {nameof(ProgramResponse.DurationMonths)},
            source_url AS {nameof(ProgramResponse.SourceUrl)},
            checked_on AS {nameof(ProgramResponse.CheckedOn)},
            checked_on < @StaleBefore AS {nameof(ProgramResponse.IsStale)}
        FROM catalogue.programs
        WHERE university_id = {UniversityIdBySlug}
        ORDER BY degree_level, name;
        """;

    private const string RequirementsSql =
        $"""
        SELECT
            program_id AS {nameof(AdmissionRequirementResponse.ProgramId)},
            intake_term AS {nameof(AdmissionRequirementResponse.IntakeTerm)},
            intake_year AS {nameof(AdmissionRequirementResponse.IntakeYear)},
            english_minimums AS {nameof(AdmissionRequirementResponse.EnglishMinimums)},
            sat_policy AS {nameof(AdmissionRequirementResponse.SatPolicy)},
            sat_mid_range_low AS {nameof(AdmissionRequirementResponse.SatMidRangeLow)},
            sat_mid_range_high AS {nameof(AdmissionRequirementResponse.SatMidRangeHigh)},
            minimum_gpa AS {nameof(AdmissionRequirementResponse.MinimumGpa)},
            gpa_scale AS {nameof(AdmissionRequirementResponse.GpaScale)},
            documents AS {nameof(AdmissionRequirementResponse.Documents)},
            essays_count AS {nameof(AdmissionRequirementResponse.EssaysCount)},
            interview_required AS {nameof(AdmissionRequirementResponse.InterviewRequired)},
            source_url AS {nameof(AdmissionRequirementResponse.SourceUrl)},
            checked_on AS {nameof(AdmissionRequirementResponse.CheckedOn)},
            checked_on < @StaleBefore AS {nameof(AdmissionRequirementResponse.IsStale)}
        FROM catalogue.admission_requirements
        WHERE program_id IN {ProgramIdsBySlug}
        ORDER BY intake_year, intake_term;
        """;

    private const string DeadlinesSql =
        $"""
        SELECT
            program_id AS {nameof(DeadlineResponse.ProgramId)},
            round AS {nameof(DeadlineResponse.Round)},
            due_date AS {nameof(DeadlineResponse.DueDate)},
            source_url AS {nameof(DeadlineResponse.SourceUrl)},
            checked_on AS {nameof(DeadlineResponse.CheckedOn)},
            checked_on < @StaleBefore AS {nameof(DeadlineResponse.IsStale)}
        FROM catalogue.deadlines
        WHERE program_id IN {ProgramIdsBySlug}
        ORDER BY due_date;
        """;

    private const string CostsSql =
        $"""
        SELECT
            academic_year AS {nameof(CostResponse.AcademicYear)},
            tuition_minor AS {nameof(CostResponse.TuitionMinor)},
            living_minor AS {nameof(CostResponse.LivingMinor)},
            currency AS {nameof(CostResponse.Currency)},
            source_url AS {nameof(CostResponse.SourceUrl)},
            checked_on AS {nameof(CostResponse.CheckedOn)},
            checked_on < @StaleBefore AS {nameof(CostResponse.IsStale)}
        FROM catalogue.costs
        WHERE university_id = {UniversityIdBySlug}
        ORDER BY academic_year DESC;
        """;

    private const string ScholarshipsSql =
        $"""
        SELECT
            name AS {nameof(ScholarshipResponse.Name)},
            kind AS {nameof(ScholarshipResponse.Kind)},
            amount AS {nameof(ScholarshipResponse.Amount)},
            eligibility AS {nameof(ScholarshipResponse.Eligibility)},
            source_url AS {nameof(ScholarshipResponse.SourceUrl)},
            checked_on AS {nameof(ScholarshipResponse.CheckedOn)},
            checked_on < @StaleBefore AS {nameof(ScholarshipResponse.IsStale)}
        FROM catalogue.scholarships
        WHERE university_id = {UniversityIdBySlug}
        ORDER BY name;
        """;

    public static async Task<UniversityResponse?> ReadBySlugAsync(
        DbConnection connection,
        string slug,
        DateOnly staleBefore,
        CancellationToken cancellationToken)
    {
        const string sql = UniversitySql + ProgramsSql + RequirementsSql + DeadlinesSql + CostsSql + ScholarshipsSql;

        await using var grid = await connection.QueryMultipleAsync(
            new CommandDefinition(sql, new { Slug = slug, StaleBefore = staleBefore }, cancellationToken: cancellationToken));

        var university = await grid.ReadSingleOrDefaultAsync<UniversityResponse>();
        var programs = (await grid.ReadAsync<ProgramResponse>()).AsList();
        var requirements = (await grid.ReadAsync<AdmissionRequirementResponse>()).ToLookup(x => x.ProgramId);
        var deadlines = (await grid.ReadAsync<DeadlineResponse>()).ToLookup(x => x.ProgramId);
        var costs = (await grid.ReadAsync<CostResponse>()).AsList();
        var scholarships = (await grid.ReadAsync<ScholarshipResponse>()).AsList();

        if (university is null)
        {
            return null;
        }

        return university with
        {
            Programs = [.. programs.Select(program => program with
            {
                Requirements = [.. requirements[program.Id]],
                Deadlines = [.. deadlines[program.Id]]
            })],
            Costs = costs,
            Scholarships = scholarships
        };
    }
}
