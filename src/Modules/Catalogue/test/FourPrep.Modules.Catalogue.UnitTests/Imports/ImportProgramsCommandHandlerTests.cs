namespace FourPrep.Modules.Catalogue.UnitTests.Imports;

using FourPrep.Common.Domain.Enums;
using FourPrep.Modules.Catalogue.Domain.Imports;
using FourPrep.Modules.Catalogue.Application.Programs;
using FourPrep.Modules.Catalogue.Application.Imports;
using FourPrep.Modules.Catalogue.Application.Imports.ImportPrograms;

public class ImportProgramsCommandHandlerTests
{
    private const string Header = "university_slug,name,degree_level,field,language,duration_months,source_url,checked_on";
    private const string ComputerScience = "MIT,BSc Computer Science,bachelor,Computer Science,EN,48,https://www.mit.edu/cs,2026-10-01";
    private const string Physics = "mit,MSc Physics,Master,Physics,en,24,https://www.mit.edu/physics,2026-10-01";

    [Fact]
    public async Task Handle_CreatesNewProgramsAndUpdatesTheOnesWithTheSameName()
    {
        var universities = new FakeUniversityRepository();
        var university = universities.Add("mit");
        var programs = new FakeProgramRepository();
        var existing = programs.Add(university.Id, "BSc Computer Science");
        var unitOfWork = new FakeUnitOfWork();

        var result = await Handler(universities, programs, unitOfWork, Header, ComputerScience, Physics).Handle(Command(), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(new ImportResponse(2, 1, 1), result.Data);
        Assert.Equal(new FakeClock().UtcNow, existing.UpdatedOnUtc);
        var created = Assert.Single(programs.Programs, program => program.Name == "MSc Physics");
        Assert.Equal(DegreeLevel.Master, created.DegreeLevel);
        Assert.Equal(university.Id, created.UniversityId);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_ReportsAnUnknownUniversityAndSavesNothing()
    {
        var programs = new FakeProgramRepository();
        var unitOfWork = new FakeUnitOfWork();

        var result = await Handler(new FakeUniversityRepository(), programs, unitOfWork, Header, ComputerScience).Handle(Command(), default);

        ImportAssert.HasError(result, "rows[2].university_slug", ImportErrors.UnknownUniversity);
        Assert.Empty(programs.Programs);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_ReportsAProgramRepeatedInTheFile()
    {
        var universities = new FakeUniversityRepository();
        universities.Add("mit");

        var result = await Handler(universities, new FakeProgramRepository(), new FakeUnitOfWork(), Header, ComputerScience, ComputerScience)
            .Handle(Command(), default);

        ImportAssert.HasError(result, "rows[3].name", ImportErrors.DuplicateRow);
    }

    [Fact]
    public async Task Handle_ReportsAnUnknownDegreeLevel()
    {
        var universities = new FakeUniversityRepository();
        universities.Add("mit");
        string row = "mit,BSc Computer Science,Bachelors,Computer Science,en,48,https://www.mit.edu/cs,2026-10-01";

        var result = await Handler(universities, new FakeProgramRepository(), new FakeUnitOfWork(), Header, row).Handle(Command(), default);

        ImportAssert.HasError(result, "rows[2].degree_level", ImportErrors.InvalidValue);
    }

    private static ImportProgramsCommand Command() => new(Stream.Null);

    private static ImportProgramsCommandHandler Handler(
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
            new ProgramDetailsValidator(new FakeTimeZoneContext()),
            new FakeClock());
}
