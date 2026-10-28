namespace FourPrep.Modules.Catalogue.UnitTests.Universities;

using FourPrep.Common.Domain.Enums;
using FourPrep.Modules.Catalogue.Domain.Universities;
using FourPrep.Modules.Catalogue.Application.Universities;
using FourPrep.Modules.Catalogue.Application.Universities.CreateUniversity;
using FourPrep.Modules.Catalogue.Application.Universities.UpdateUniversity;
using FourPrep.Modules.Catalogue.Application.Universities.DeleteUniversity;
using FourPrep.Modules.Catalogue.Application.Universities.ReplaceUniversityCosts;
using FourPrep.Modules.Catalogue.Application.Universities.ReplaceUniversityScholarships;

public class UniversityCommandHandlerTests
{
    private static readonly Guid StaffUserId = Guid.NewGuid();

    internal static UniversityDetails Details(string slug = "tu-munich") =>
        new(
            slug,
            " Technical University of Munich ",
            "de",
            " Munich ",
            "https://www.tum.de",
            "QS World University Rankings 2026",
            26,
            8.5m,
            true,
            new Dictionary<string, string> { ["EN"] = " A public research university. ", ["uz"] = " " },
            " https://www.tum.de/en/studies ",
            new DateOnly(2026, 10, 1));

    [Fact]
    public async Task Create_SavesANormalizedUniversityCheckedByTheCurrentUser()
    {
        var repository = new FakeUniversityRepository();
        var unitOfWork = new FakeUnitOfWork();

        var result = await CreateHandler(repository, unitOfWork).Handle(new CreateUniversityCommand(Details()), default);

        Assert.True(result.IsSuccess);
        var university = Assert.Single(repository.Universities);
        Assert.Equal(result.Data, university.Id);
        Assert.Equal("Technical University of Munich", university.Name);
        Assert.Equal("DE", university.CountryCode);
        Assert.Equal("Munich", university.City);
        Assert.Equal("https://www.tum.de/en/studies", university.SourceUrl);
        Assert.Equal(new Dictionary<string, string> { ["en"] = "A public research university." }, university.Description);
        Assert.Equal(StaffUserId, university.CheckedByUserId);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Create_RefusesASlugAnotherUniversityUses()
    {
        var repository = new FakeUniversityRepository();
        repository.Add("tu-munich");
        var unitOfWork = new FakeUnitOfWork();

        var result = await CreateHandler(repository, unitOfWork).Handle(new CreateUniversityCommand(Details()), default);

        Assert.Equal(UniversityErrors.SlugTaken, result.Error);
        Assert.Single(repository.Universities);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Update_ChangesTheUniversityAndKeepsItsOwnSlug()
    {
        var repository = new FakeUniversityRepository();
        var university = repository.Add("tu-munich");
        var unitOfWork = new FakeUnitOfWork();

        var result = await UpdateHandler(repository, unitOfWork)
            .Handle(new UpdateUniversityCommand(university.Id, Details("tu-munich")), default);

        Assert.True(result.IsSuccess);
        Assert.Equal("Technical University of Munich", university.Name);
        Assert.Equal(26, university.Ranking);
        Assert.Equal(new FakeClock().UtcNow, university.UpdatedOnUtc);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Update_RefusesASlugAnotherUniversityUses()
    {
        var repository = new FakeUniversityRepository();
        repository.Add("tu-munich");
        var university = repository.Add("mit");
        var unitOfWork = new FakeUnitOfWork();

        var result = await UpdateHandler(repository, unitOfWork)
            .Handle(new UpdateUniversityCommand(university.Id, Details("tu-munich")), default);

        Assert.Equal(UniversityErrors.SlugTaken, result.Error);
        Assert.Equal("mit", university.Slug);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Update_ReportsAnUnknownUniversity()
    {
        var unitOfWork = new FakeUnitOfWork();

        var result = await UpdateHandler(new FakeUniversityRepository(), unitOfWork)
            .Handle(new UpdateUniversityCommand(Guid.NewGuid(), Details()), default);

        Assert.Equal(UniversityErrors.NotFound, result.Error);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Delete_RemovesTheUniversity()
    {
        var repository = new FakeUniversityRepository();
        var university = repository.Add();
        var unitOfWork = new FakeUnitOfWork();

        var result = await new DeleteUniversityCommandHandler(unitOfWork, repository)
            .Handle(new DeleteUniversityCommand(university.Id), default);

        Assert.True(result.IsSuccess);
        Assert.Empty(repository.Universities);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Delete_ReportsAnUnknownUniversity()
    {
        var unitOfWork = new FakeUnitOfWork();

        var result = await new DeleteUniversityCommandHandler(unitOfWork, new FakeUniversityRepository())
            .Handle(new DeleteUniversityCommand(Guid.NewGuid()), default);

        Assert.Equal(UniversityErrors.NotFound, result.Error);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task ReplaceCosts_ReplacesEveryCostAndRecordsWhoChecked()
    {
        var repository = new FakeUniversityRepository();
        var university = repository.Add();
        var unitOfWork = new FakeUnitOfWork();
        var handler = CostsHandler(repository, unitOfWork);
        await handler.Handle(Costs(university.Id, 2025), default);

        var result = await handler.Handle(Costs(university.Id, 2026), default);

        Assert.True(result.IsSuccess);
        var cost = Assert.Single(university.Costs);
        Assert.Equal(2026, cost.AcademicYear);
        Assert.Equal("EUR", cost.Currency);
        Assert.Equal(StaffUserId, university.CheckedByUserId);
        Assert.Equal(2, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task ReplaceCosts_ReportsAnUnknownUniversity()
    {
        var unitOfWork = new FakeUnitOfWork();

        var result = await CostsHandler(new FakeUniversityRepository(), unitOfWork).Handle(Costs(Guid.NewGuid(), 2026), default);

        Assert.Equal(UniversityErrors.NotFound, result.Error);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task ReplaceScholarships_StoresTheScholarshipsWithNormalizedText()
    {
        var repository = new FakeUniversityRepository();
        var university = repository.Add();
        var unitOfWork = new FakeUnitOfWork();
        var command = new ReplaceUniversityScholarshipsCommand(
            university.Id,
            [
                new ScholarshipInput(
                    " Deutschlandstipendium ",
                    ScholarshipKind.Merit,
                    new Dictionary<string, string> { ["en"] = " EUR 300 per month " },
                    new Dictionary<string, string> { ["en"] = "Outstanding students", ["RU"] = "Отличники" },
                    "https://www.tum.de/stipendium",
                    new DateOnly(2026, 9, 1))
            ]);

        var result = await new ReplaceUniversityScholarshipsCommandHandler(unitOfWork, new FakeCurrentUser(StaffUserId), repository, new FakeClock())
            .Handle(command, default);

        Assert.True(result.IsSuccess);
        var scholarship = Assert.Single(university.Scholarships);
        Assert.Equal("Deutschlandstipendium", scholarship.Name);
        Assert.Equal("EUR 300 per month", scholarship.Amount["en"]);
        Assert.Equal("Отличники", scholarship.Eligibility["ru"]);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task ReplaceScholarships_ReportsAnUnknownUniversity()
    {
        var unitOfWork = new FakeUnitOfWork();

        var result = await new ReplaceUniversityScholarshipsCommandHandler(unitOfWork, new FakeCurrentUser(StaffUserId), new FakeUniversityRepository(), new FakeClock())
            .Handle(new ReplaceUniversityScholarshipsCommand(Guid.NewGuid(), []), default);

        Assert.Equal(UniversityErrors.NotFound, result.Error);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    private static ReplaceUniversityCostsCommand Costs(Guid universityId, int academicYear) =>
        new(universityId, [new CostInput(academicYear, 0, 1_200_000, " eur ", "https://www.tum.de/fees", new DateOnly(2026, 9, 1))]);

    private static CreateUniversityCommandHandler CreateHandler(FakeUniversityRepository repository, FakeUnitOfWork unitOfWork) =>
        new(unitOfWork, new FakeCurrentUser(StaffUserId), repository, new FakeClock());

    private static UpdateUniversityCommandHandler UpdateHandler(FakeUniversityRepository repository, FakeUnitOfWork unitOfWork) =>
        new(unitOfWork, new FakeCurrentUser(StaffUserId), repository, new FakeClock());

    private static ReplaceUniversityCostsCommandHandler CostsHandler(FakeUniversityRepository repository, FakeUnitOfWork unitOfWork) =>
        new(unitOfWork, new FakeCurrentUser(StaffUserId), repository, new FakeClock());
}
