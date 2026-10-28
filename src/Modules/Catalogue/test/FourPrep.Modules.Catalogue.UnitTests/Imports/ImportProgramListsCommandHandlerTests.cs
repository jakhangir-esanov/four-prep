namespace FourPrep.Modules.Catalogue.UnitTests.Imports;

using FourPrep.Common.Domain.Enums;
using FourPrep.Modules.Catalogue.Domain.Imports;
using FourPrep.Modules.Catalogue.Domain.Deadlines;
using FourPrep.Modules.Catalogue.Application.Imports;
using FourPrep.Modules.Catalogue.Application.Imports.ImportDeadlines;
using FourPrep.Modules.Catalogue.Application.Imports.ImportRequirements;
using FourPrep.Modules.Catalogue.Application.Programs.ReplaceProgramDeadlines;
using FourPrep.Modules.Catalogue.Application.Programs.ReplaceProgramRequirements;

public class ImportProgramListsCommandHandlerTests
{
    private const string RequirementHeader =
        "university_slug,program_name,intake_term,intake_year,ielts_min,toefl_min,sat_policy,sat_mid_range_low,sat_mid_range_high,documents,interview_required,source_url,checked_on";

    private const string DeadlineHeader = "university_slug,program_name,round,due_date,source_url,checked_on";

    [Fact]
    public async Task ImportRequirements_ReadsExamMinimumsAndDocumentLists()
    {
        var (universities, programs) = Catalogue();
        var program = programs.Programs[0];
        var unitOfWork = new FakeUnitOfWork();

        var result = await RequirementsHandler(
            universities,
            programs,
            unitOfWork,
            RequirementHeader,
            "mit,BSc Computer Science,fall,2027,7,100,Required,1520,1580,Transcript; Two recommendations,false,https://mitadmissions.org,2026-10-01")
            .Handle(new ImportRequirementsCommand(Stream.Null), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(new ImportResponse(1, 0, 1), result.Data);
        var requirement = Assert.Single(program.Requirements);
        Assert.Equal(IntakeTerm.Fall, requirement.IntakeTerm);
        Assert.Equal(new Dictionary<Exam, decimal> { [Exam.Ielts] = 7, [Exam.Toefl] = 100 }, requirement.EnglishMinimums);
        Assert.Equal(SatPolicy.Required, requirement.SatPolicy);
        Assert.Equal(["Transcript", "Two recommendations"], requirement.Documents);
        Assert.False(requirement.InterviewRequired);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task ImportRequirements_ReportsAProgramTheUniversityDoesNotHave()
    {
        var (universities, programs) = Catalogue();
        var unitOfWork = new FakeUnitOfWork();

        var result = await RequirementsHandler(
            universities,
            programs,
            unitOfWork,
            RequirementHeader,
            "mit,BSc Biology,fall,2027,,,,,,,,https://mitadmissions.org,2026-10-01")
            .Handle(new ImportRequirementsCommand(Stream.Null), default);

        ImportAssert.HasError(result, "rows[2].program_name", ImportErrors.UnknownProgram);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task ImportRequirements_ReportsAColumnForAnExamThePlatformDoesNotKnow()
    {
        var (universities, programs) = Catalogue();

        var result = await RequirementsHandler(
            universities,
            programs,
            new FakeUnitOfWork(),
            "university_slug,program_name,intake_term,intake_year,gmat_min,source_url,checked_on",
            "mit,BSc Computer Science,fall,2027,600,https://mitadmissions.org,2026-10-01")
            .Handle(new ImportRequirementsCommand(Stream.Null), default);

        ImportAssert.HasError(result, "columns.gmat_min", ImportErrors.UnknownColumn);
    }

    [Fact]
    public async Task ImportDeadlines_ReplacesTheDeadlinesOfTheProgram()
    {
        var (universities, programs) = Catalogue();
        var program = programs.Programs[0];
        var unitOfWork = new FakeUnitOfWork();

        var result = await DeadlinesHandler(
            universities,
            programs,
            unitOfWork,
            DeadlineHeader,
            "mit,BSc Computer Science,Early Action,2026-11-01,https://mitadmissions.org/dates,2026-10-01",
            "mit,BSc Computer Science,Regular Action,2027-01-05,https://mitadmissions.org/dates,2026-10-01")
            .Handle(new ImportDeadlinesCommand(Stream.Null), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(["Early Action", "Regular Action"], program.Deadlines.Select(deadline => deadline.Round));
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task ImportDeadlines_ReportsARoundRepeatedForOneProgram()
    {
        var (universities, programs) = Catalogue();

        var result = await DeadlinesHandler(
            universities,
            programs,
            new FakeUnitOfWork(),
            DeadlineHeader,
            "mit,BSc Computer Science,Early Action,2026-11-01,https://mitadmissions.org/dates,2026-10-01",
            "mit,BSc Computer Science,early action,2026-11-15,https://mitadmissions.org/dates,2026-10-01")
            .Handle(new ImportDeadlinesCommand(Stream.Null), default);

        ImportAssert.HasError(result, "rows[2].round", DeadlineErrors.DuplicateRound);
    }

    [Fact]
    public async Task ImportDeadlines_ReportsADateInAnotherFormat()
    {
        var (universities, programs) = Catalogue();

        var result = await DeadlinesHandler(
            universities,
            programs,
            new FakeUnitOfWork(),
            DeadlineHeader,
            "mit,BSc Computer Science,Early Action,01.11.2026,https://mitadmissions.org/dates,2026-10-01")
            .Handle(new ImportDeadlinesCommand(Stream.Null), default);

        ImportAssert.HasError(result, "rows[2].due_date", ImportErrors.InvalidValue);
    }

    private static (FakeUniversityRepository Universities, FakeProgramRepository Programs) Catalogue()
    {
        var universities = new FakeUniversityRepository();
        var programs = new FakeProgramRepository();
        programs.Add(universities.Add("mit").Id, "BSc Computer Science");
        return (universities, programs);
    }

    private static ImportRequirementsCommandHandler RequirementsHandler(
        FakeUniversityRepository universities,
        FakeProgramRepository programs,
        FakeUnitOfWork unitOfWork,
        params string[] lines) =>
        new(
            new FakeCsvReader(lines),
            unitOfWork,
            new FakeCurrentUser(Guid.NewGuid()),
            universities,
            programs,
            new ReplaceProgramRequirementsCommandValidator(new FakeTimeZoneContext()),
            new FakeClock());

    private static ImportDeadlinesCommandHandler DeadlinesHandler(
        FakeUniversityRepository universities,
        FakeProgramRepository programs,
        FakeUnitOfWork unitOfWork,
        params string[] lines) =>
        new(
            new FakeCsvReader(lines),
            unitOfWork,
            new FakeCurrentUser(Guid.NewGuid()),
            universities,
            programs,
            new ReplaceProgramDeadlinesCommandValidator(new FakeTimeZoneContext()),
            new FakeClock());
}
