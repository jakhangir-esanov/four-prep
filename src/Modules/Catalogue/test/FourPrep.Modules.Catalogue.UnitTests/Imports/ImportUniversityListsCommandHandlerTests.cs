namespace FourPrep.Modules.Catalogue.UnitTests.Imports;

using FourPrep.Common.Domain.Enums;
using FourPrep.Modules.Catalogue.Domain.Costs;
using FourPrep.Modules.Catalogue.Domain.Imports;
using FourPrep.Modules.Catalogue.Application.Imports;
using FourPrep.Modules.Catalogue.Application.Imports.ImportCosts;
using FourPrep.Modules.Catalogue.Application.Imports.ImportScholarships;
using FourPrep.Modules.Catalogue.Application.Universities.ReplaceUniversityCosts;
using FourPrep.Modules.Catalogue.Application.Universities.ReplaceUniversityScholarships;

public class ImportUniversityListsCommandHandlerTests
{
    private const string CostHeader = "university_slug,academic_year,tuition_minor,living_minor,currency,source_url,checked_on";

    [Fact]
    public async Task ImportCosts_ReplacesTheCostsOfEachUniversityInTheFile()
    {
        var universities = new FakeUniversityRepository();
        var mit = universities.Add("mit");
        var tum = universities.Add("tu-munich");
        var untouched = universities.Add("harvard");
        await CostsHandler(universities, new FakeUnitOfWork(), CostHeader, "harvard,2025,100,,usd,https://www.harvard.edu,2026-09-01")
            .Handle(new ImportCostsCommand(Stream.Null), default);
        var unitOfWork = new FakeUnitOfWork();

        var result = await CostsHandler(
            universities,
            unitOfWork,
            CostHeader,
            "mit,2025,5900000,2000000,usd,https://sfs.mit.edu,2026-09-01",
            "mit,2026,6100000,,USD,https://sfs.mit.edu,2026-10-01",
            "tu-munich,2026,0,1200000,eur,https://www.tum.de/fees,2026-10-01").Handle(new ImportCostsCommand(Stream.Null), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(new ImportResponse(3, 0, 2), result.Data);
        Assert.Equal([2025, 2026], mit.Costs.Select(cost => cost.AcademicYear).Order());
        Assert.Equal("EUR", Assert.Single(tum.Costs).Currency);
        Assert.Single(untouched.Costs);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task ImportCosts_ReportsTheRowOfABadAmount()
    {
        var universities = new FakeUniversityRepository();
        universities.Add("mit");
        var unitOfWork = new FakeUnitOfWork();

        var result = await CostsHandler(
            universities,
            unitOfWork,
            CostHeader,
            "mit,2025,5900000,,usd,https://sfs.mit.edu,2026-09-01",
            "mit,2026,-1,,usd,https://sfs.mit.edu,2026-09-01").Handle(new ImportCostsCommand(Stream.Null), default);

        ImportAssert.HasError(result, "rows[3].tuition_minor", CostErrors.InvalidAmount);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task ImportCosts_ReportsAnAcademicYearRepeatedForOneUniversity()
    {
        var universities = new FakeUniversityRepository();
        universities.Add("mit");

        var result = await CostsHandler(
            universities,
            new FakeUnitOfWork(),
            CostHeader,
            "mit,2026,5900000,,usd,https://sfs.mit.edu,2026-09-01",
            "mit,2026,6100000,,usd,https://sfs.mit.edu,2026-09-01").Handle(new ImportCostsCommand(Stream.Null), default);

        ImportAssert.HasError(result, "rows[2].academic_year", CostErrors.DuplicateAcademicYear);
    }

    [Fact]
    public async Task ImportCosts_ReportsAnUnknownUniversity()
    {
        var unitOfWork = new FakeUnitOfWork();

        var result = await CostsHandler(
            new FakeUniversityRepository(),
            unitOfWork,
            CostHeader,
            "oxford,2026,3000000,,gbp,https://www.ox.ac.uk,2026-09-01").Handle(new ImportCostsCommand(Stream.Null), default);

        ImportAssert.HasError(result, "rows[2].university_slug", ImportErrors.UnknownUniversity);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task ImportScholarships_ReadsTranslatedColumnsAndTheKind()
    {
        var universities = new FakeUniversityRepository();
        var mit = universities.Add("mit");
        var unitOfWork = new FakeUnitOfWork();
        var handler = new ImportScholarshipsCommandHandler(
            new FakeCsvReader(
                "university_slug,name,kind,amount_en,eligibility_en,eligibility_ru,source_url,checked_on",
                "mit,MIT Scholarship,needbased,Up to full tuition,Need-based,По нуждаемости,https://sfs.mit.edu,2026-09-01"),
            unitOfWork,
            new FakeCurrentUser(Guid.NewGuid()),
            universities,
            new FakeProgramRepository(),
            new ReplaceUniversityScholarshipsCommandValidator(new FakeTimeZoneContext()),
            new FakeClock());

        var result = await handler.Handle(new ImportScholarshipsCommand(Stream.Null), default);

        Assert.True(result.IsSuccess);
        var scholarship = Assert.Single(mit.Scholarships);
        Assert.Equal(ScholarshipKind.NeedBased, scholarship.Kind);
        Assert.Equal("Up to full tuition", scholarship.Amount["en"]);
        Assert.Equal("По нуждаемости", scholarship.Eligibility["ru"]);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    private static ImportCostsCommandHandler CostsHandler(
        FakeUniversityRepository universities,
        FakeUnitOfWork unitOfWork,
        params string[] lines) =>
        new(
            new FakeCsvReader(lines),
            unitOfWork,
            new FakeCurrentUser(Guid.NewGuid()),
            universities,
            new FakeProgramRepository(),
            new ReplaceUniversityCostsCommandValidator(new FakeTimeZoneContext()),
            new FakeClock());
}
