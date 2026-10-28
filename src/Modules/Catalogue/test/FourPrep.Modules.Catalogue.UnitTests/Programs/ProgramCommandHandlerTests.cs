namespace FourPrep.Modules.Catalogue.UnitTests.Programs;

using FourPrep.Common.Domain.Enums;
using FourPrep.Modules.Catalogue.Domain.Programs;
using FourPrep.Modules.Catalogue.Domain.Universities;
using FourPrep.Modules.Catalogue.Application.Programs;
using FourPrep.Modules.Catalogue.Application.Programs.CreateProgram;
using FourPrep.Modules.Catalogue.Application.Programs.UpdateProgram;
using FourPrep.Modules.Catalogue.Application.Programs.DeleteProgram;
using FourPrep.Modules.Catalogue.Application.Programs.ReplaceProgramDeadlines;
using FourPrep.Modules.Catalogue.Application.Programs.ReplaceProgramRequirements;

public class ProgramCommandHandlerTests
{
    private static readonly Guid StaffUserId = Guid.NewGuid();

    internal static ProgramDetails Details() =>
        new(" MSc Informatics ", DegreeLevel.Master, " Computer Science ", "DE", 24, "https://www.tum.de/informatics", new DateOnly(2026, 10, 1));

    internal static AdmissionRequirementInput Requirement() =>
        new(
            IntakeTerm.Winter,
            2027,
            new Dictionary<Exam, decimal> { [Exam.Ielts] = 6.5m, [Exam.Toefl] = 88 },
            SatPolicy.NotConsidered,
            null,
            null,
            2.5m,
            4.0m,
            [" Transcript ", "CV", " "],
            1,
            false,
            "https://www.tum.de/informatics/apply",
            new DateOnly(2026, 10, 1));

    [Fact]
    public async Task Create_AddsAProgramToAnExistingUniversity()
    {
        var universities = new FakeUniversityRepository();
        var university = universities.Add();
        var programs = new FakeProgramRepository();
        var unitOfWork = new FakeUnitOfWork();

        var result = await new CreateProgramCommandHandler(unitOfWork, new FakeCurrentUser(StaffUserId), programs, universities, new FakeClock())
            .Handle(new CreateProgramCommand(university.Id, Details()), default);

        Assert.True(result.IsSuccess);
        var program = Assert.Single(programs.Programs);
        Assert.Equal(result.Data, program.Id);
        Assert.Equal(university.Id, program.UniversityId);
        Assert.Equal("MSc Informatics", program.Name);
        Assert.Equal("Computer Science", program.Field);
        Assert.Equal("de", program.Language);
        Assert.Equal(StaffUserId, program.CheckedByUserId);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Create_ReportsAnUnknownUniversity()
    {
        var programs = new FakeProgramRepository();
        var unitOfWork = new FakeUnitOfWork();

        var result = await new CreateProgramCommandHandler(unitOfWork, new FakeCurrentUser(StaffUserId), programs, new FakeUniversityRepository(), new FakeClock())
            .Handle(new CreateProgramCommand(Guid.NewGuid(), Details()), default);

        Assert.Equal(UniversityErrors.NotFound, result.Error);
        Assert.Empty(programs.Programs);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Update_ChangesTheProgram()
    {
        var programs = new FakeProgramRepository();
        var program = programs.Add(Guid.NewGuid());
        var unitOfWork = new FakeUnitOfWork();

        var result = await new UpdateProgramCommandHandler(unitOfWork, new FakeCurrentUser(StaffUserId), programs, new FakeClock())
            .Handle(new UpdateProgramCommand(program.Id, Details()), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(DegreeLevel.Master, program.DegreeLevel);
        Assert.Equal(24, program.DurationMonths);
        Assert.Equal(new FakeClock().UtcNow, program.UpdatedOnUtc);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Update_ReportsAnUnknownProgram()
    {
        var unitOfWork = new FakeUnitOfWork();

        var result = await new UpdateProgramCommandHandler(unitOfWork, new FakeCurrentUser(StaffUserId), new FakeProgramRepository(), new FakeClock())
            .Handle(new UpdateProgramCommand(Guid.NewGuid(), Details()), default);

        Assert.Equal(ProgramErrors.NotFound, result.Error);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Delete_RemovesTheProgram()
    {
        var programs = new FakeProgramRepository();
        var program = programs.Add(Guid.NewGuid());
        var unitOfWork = new FakeUnitOfWork();

        var result = await new DeleteProgramCommandHandler(unitOfWork, programs).Handle(new DeleteProgramCommand(program.Id), default);

        Assert.True(result.IsSuccess);
        Assert.Empty(programs.Programs);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Delete_ReportsAnUnknownProgram()
    {
        var unitOfWork = new FakeUnitOfWork();

        var result = await new DeleteProgramCommandHandler(unitOfWork, new FakeProgramRepository())
            .Handle(new DeleteProgramCommand(Guid.NewGuid()), default);

        Assert.Equal(ProgramErrors.NotFound, result.Error);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task ReplaceRequirements_StoresTheRequirementsWithCleanDocuments()
    {
        var programs = new FakeProgramRepository();
        var program = programs.Add(Guid.NewGuid());
        var unitOfWork = new FakeUnitOfWork();

        var result = await RequirementsHandler(programs, unitOfWork)
            .Handle(new ReplaceProgramRequirementsCommand(program.Id, [Requirement()]), default);

        Assert.True(result.IsSuccess);
        var requirement = Assert.Single(program.Requirements);
        Assert.Equal(6.5m, requirement.EnglishMinimums[Exam.Ielts]);
        Assert.Equal(["Transcript", "CV"], requirement.Documents);
        Assert.Equal(StaffUserId, program.CheckedByUserId);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task ReplaceRequirements_DropsTheRequirementsThatAreNoLongerSent()
    {
        var programs = new FakeProgramRepository();
        var program = programs.Add(Guid.NewGuid());
        var unitOfWork = new FakeUnitOfWork();
        var handler = RequirementsHandler(programs, unitOfWork);
        await handler.Handle(new ReplaceProgramRequirementsCommand(program.Id, [Requirement()]), default);

        var result = await handler.Handle(new ReplaceProgramRequirementsCommand(program.Id, []), default);

        Assert.True(result.IsSuccess);
        Assert.Empty(program.Requirements);
    }

    [Fact]
    public async Task ReplaceRequirements_ReportsAnUnknownProgram()
    {
        var unitOfWork = new FakeUnitOfWork();

        var result = await RequirementsHandler(new FakeProgramRepository(), unitOfWork)
            .Handle(new ReplaceProgramRequirementsCommand(Guid.NewGuid(), [Requirement()]), default);

        Assert.Equal(ProgramErrors.NotFound, result.Error);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task ReplaceDeadlines_StoresTheDeadlines()
    {
        var programs = new FakeProgramRepository();
        var program = programs.Add(Guid.NewGuid());
        var unitOfWork = new FakeUnitOfWork();
        var command = new ReplaceProgramDeadlinesCommand(
            program.Id,
            [new DeadlineInput(" Winter semester ", new DateOnly(2027, 5, 31), "https://www.tum.de/dates", new DateOnly(2026, 10, 1))]);

        var result = await new ReplaceProgramDeadlinesCommandHandler(unitOfWork, new FakeCurrentUser(StaffUserId), programs, new FakeClock())
            .Handle(command, default);

        Assert.True(result.IsSuccess);
        var deadline = Assert.Single(program.Deadlines);
        Assert.Equal("Winter semester", deadline.Round);
        Assert.Equal(new DateOnly(2027, 5, 31), deadline.DueDate);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task ReplaceDeadlines_ReportsAnUnknownProgram()
    {
        var unitOfWork = new FakeUnitOfWork();

        var result = await new ReplaceProgramDeadlinesCommandHandler(unitOfWork, new FakeCurrentUser(StaffUserId), new FakeProgramRepository(), new FakeClock())
            .Handle(new ReplaceProgramDeadlinesCommand(Guid.NewGuid(), []), default);

        Assert.Equal(ProgramErrors.NotFound, result.Error);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    private static ReplaceProgramRequirementsCommandHandler RequirementsHandler(FakeProgramRepository programs, FakeUnitOfWork unitOfWork) =>
        new(unitOfWork, new FakeCurrentUser(StaffUserId), programs, new FakeClock());
}
