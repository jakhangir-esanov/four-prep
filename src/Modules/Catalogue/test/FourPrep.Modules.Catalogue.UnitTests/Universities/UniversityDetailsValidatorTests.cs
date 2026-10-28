namespace FourPrep.Modules.Catalogue.UnitTests.Universities;

using FourPrep.Common.Domain.Localization;
using FourPrep.Modules.Catalogue.Domain.Facts;
using FourPrep.Modules.Catalogue.Domain.Universities;
using FourPrep.Modules.Catalogue.Application.Universities;
using FourPrep.Modules.Catalogue.Application.Universities.CreateUniversity;

public class UniversityDetailsValidatorTests
{
    private static readonly CreateUniversityCommandValidator Validator = new(new FakeTimeZoneContext());

    [Fact]
    public void Validate_WhenTheUniversityIsComplete_Passes()
    {
        Assert.True(Validator.Validate(Command(UniversityCommandHandlerTests.Details())).IsValid);
    }

    [Fact]
    public void Validate_WhenOnlyTheRequiredFieldsAreGiven_Passes()
    {
        var details = new UniversityDetails(
            "mit", "MIT", "US", null, null, null, null, null, false, null, "https://www.mit.edu", new DateOnly(2026, 10, 23));

        Assert.True(Validator.Validate(Command(details)).IsValid);
    }

    [Theory]
    [InlineData("TU-Munich")]
    [InlineData("tu munich")]
    [InlineData("tu--munich")]
    [InlineData("-mit")]
    public void Validate_WhenTheSlugIsNotAnAddressName_FailsWithInvalidSlug(string slug)
    {
        AssertSingleError(Details() with { Slug = slug }, UniversityErrors.InvalidSlug.Code, "Details.Slug");
    }

    [Fact]
    public void Validate_WhenTheCountryIsNotAnIsoCode_FailsWithInvalidCountry()
    {
        AssertSingleError(Details() with { CountryCode = "Germany" }, LocalizationErrors.InvalidCountry.Code, "Details.CountryCode");
    }

    [Theory]
    [InlineData("www.tum.de")]
    [InlineData("ftp://tum.de")]
    public void Validate_WhenTheWebsiteIsNotAWebAddress_FailsWithInvalidWebsite(string website)
    {
        AssertSingleError(Details() with { Website = website }, UniversityErrors.InvalidWebsite.Code, "Details.Website");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(100.5)]
    public void Validate_WhenTheAcceptanceRateIsNotAPercentage_FailsWithInvalidAcceptanceRate(double rate)
    {
        AssertSingleError(
            Details() with { AcceptanceRate = (decimal)rate },
            UniversityErrors.InvalidAcceptanceRate.Code,
            "Details.AcceptanceRate");
    }

    [Fact]
    public void Validate_WhenARankingHasNoSource_FailsWithRankingSourceRequired()
    {
        AssertSingleError(
            Details() with { RankingSource = null },
            UniversityErrors.RankingSourceRequired.Code,
            "Details.RankingSource");
    }

    [Fact]
    public void Validate_WhenTheDescriptionHasNoEnglishVersion_FailsWithMissingEnglishText()
    {
        AssertSingleError(
            Details() with { Description = new Dictionary<string, string> { ["uz"] = "Davlat universiteti" } },
            FactErrors.MissingEnglishText.Code,
            "Details.Description");
    }

    [Fact]
    public void Validate_WhenTheDescriptionUsesAnUnsupportedLanguage_FailsWithUnsupportedLocale()
    {
        AssertSingleError(
            Details() with { Description = new Dictionary<string, string> { ["en"] = "University", ["de"] = "Universität" } },
            LocalizationErrors.UnsupportedLocale.Code,
            "Details.Description");
    }

    [Theory]
    [InlineData("tum.de/studies")]
    [InlineData("mailto:office@tum.de")]
    public void Validate_WhenTheSourceIsNotAWebAddress_FailsWithInvalidSourceUrl(string sourceUrl)
    {
        AssertSingleError(Details() with { SourceUrl = sourceUrl }, FactErrors.InvalidSourceUrl.Code, "Details.SourceUrl");
    }

    [Fact]
    public void Validate_WhenTheCheckDateIsInTheFuture_FailsWithCheckedOnInFuture()
    {
        AssertSingleError(
            Details() with { CheckedOn = new DateOnly(2026, 10, 24) },
            FactErrors.CheckedOnInFuture.Code,
            "Details.CheckedOn");
    }

    [Fact]
    public void Validate_WhenTheCheckDateIsMissing_Fails()
    {
        var error = Assert.Single(Validator.Validate(Command(Details() with { CheckedOn = default })).Errors);

        Assert.Equal("Details.CheckedOn", error.PropertyName);
    }

    private static UniversityDetails Details() => UniversityCommandHandlerTests.Details();

    private static CreateUniversityCommand Command(UniversityDetails details) => new(details);

    private static void AssertSingleError(UniversityDetails details, string errorCode, string propertyName)
    {
        var error = Assert.Single(Validator.Validate(Command(details)).Errors);

        Assert.Equal(errorCode, error.ErrorCode);
        Assert.Equal(propertyName, error.PropertyName);
    }
}
