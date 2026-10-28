namespace FourPrep.Modules.Catalogue.UnitTests.Imports;

using FourPrep.Common.Domain.Localization;
using FourPrep.Modules.Catalogue.Domain.Facts;
using FourPrep.Modules.Catalogue.Domain.Imports;
using FourPrep.Modules.Catalogue.Application.Universities;
using FourPrep.Modules.Catalogue.Application.Imports.ImportUniversities;

public class ImportUniversitiesCommandHandlerTests
{
    private const string Header = "slug,name,country_code,city,ranking,ranking_source,source_url,checked_on,description_en,description_uz";
    private const string Mit = "mit,Massachusetts Institute of Technology,us,Cambridge,1,QS World University Rankings 2026,https://www.mit.edu,2026-10-01,Research university,Tadqiqot universiteti";
    private const string Tum = "tu-munich,Technical University of Munich,DE,Munich,,,https://www.tum.de,2026-09-15,Public university,";

    private static readonly Guid StaffUserId = Guid.NewGuid();

    [Fact]
    public async Task Handle_CreatesNewUniversitiesAndUpdatesExistingOnes()
    {
        var repository = new FakeUniversityRepository();
        var existing = repository.Add("mit");
        var unitOfWork = new FakeUnitOfWork();

        var result = await Handler(repository, unitOfWork, Header, Mit, Tum).Handle(Command(), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(new Application.Imports.ImportResponse(2, 1, 1), result.Data);
        Assert.Equal(2, repository.Universities.Count);
        Assert.Equal(1, existing.Ranking);
        Assert.Equal("Tadqiqot universiteti", existing.Description["uz"]);
        var created = Assert.Single(repository.Universities, university => university.Slug == "tu-munich");
        Assert.Equal("DE", created.CountryCode);
        Assert.Equal(new Dictionary<string, string> { ["en"] = "Public university" }, created.Description);
        Assert.Equal(StaffUserId, created.CheckedByUserId);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_SavesNothingWhenOneRowIsInvalid()
    {
        var repository = new FakeUniversityRepository();
        var unitOfWork = new FakeUnitOfWork();
        string broken = "harvard,Harvard University,ZZ,Cambridge,,,https://www.harvard.edu,2030-01-01,,";

        var result = await Handler(repository, unitOfWork, Header, Mit, broken).Handle(Command(), default);

        ImportAssert.HasError(result, "rows[3].country_code", LocalizationErrors.InvalidCountry);
        ImportAssert.HasError(result, "rows[3].checked_on", FactErrors.CheckedOnInFuture);
        Assert.Empty(repository.Universities);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_ReportsAValueThatCannotBeRead()
    {
        string row = "mit,MIT,US,Cambridge,first,QS,https://www.mit.edu,2026-10-01,,";

        var result = await Handler(new FakeUniversityRepository(), new FakeUnitOfWork(), Header, row).Handle(Command(), default);

        ImportAssert.HasError(result, "rows[2].ranking", ImportErrors.InvalidValue);
    }

    [Fact]
    public async Task Handle_ReportsASlugRepeatedInTheFile()
    {
        var result = await Handler(new FakeUniversityRepository(), new FakeUnitOfWork(), Header, Mit, Mit).Handle(Command(), default);

        ImportAssert.HasError(result, "rows[3].slug", ImportErrors.DuplicateRow);
    }

    [Fact]
    public async Task Handle_ReportsMissingAndUnknownColumns()
    {
        var result = await Handler(new FakeUniversityRepository(), new FakeUnitOfWork(), "slug,name,country_code,source_url,rank", "mit,MIT,US,https://www.mit.edu,1")
            .Handle(Command(), default);

        ImportAssert.HasError(result, "columns.checked_on", ImportErrors.MissingColumn);
        ImportAssert.HasError(result, "columns.rank", ImportErrors.UnknownColumn);
    }

    [Fact]
    public async Task Handle_ReportsAFileWithoutRows()
    {
        var result = await Handler(new FakeUniversityRepository(), new FakeUnitOfWork(), Header).Handle(Command(), default);

        Assert.Equal(ImportErrors.EmptyFile, result.Error);
    }

    [Fact]
    public async Task Handle_ReportsAFileThatIsNotCsv()
    {
        var unitOfWork = new FakeUnitOfWork();
        var handler = new ImportUniversitiesCommandHandler(
            new FakeCsvReader { Failure = ImportErrors.UnreadableFile },
            unitOfWork,
            new FakeCurrentUser(StaffUserId),
            new FakeUniversityRepository(),
            new UniversityDetailsValidator(new FakeTimeZoneContext()),
            new FakeClock());

        var result = await handler.Handle(Command(), default);

        Assert.Equal(ImportErrors.UnreadableFile, result.Error);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    private static ImportUniversitiesCommand Command() => new(Stream.Null);

    private static ImportUniversitiesCommandHandler Handler(
        FakeUniversityRepository repository,
        FakeUnitOfWork unitOfWork,
        params string[] lines) =>
        new(
            new FakeCsvReader(lines),
            unitOfWork,
            new FakeCurrentUser(StaffUserId),
            repository,
            new UniversityDetailsValidator(new FakeTimeZoneContext()),
            new FakeClock());
}
