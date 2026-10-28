namespace FourPrep.Modules.Catalogue.UnitTests.Universities;

using FourPrep.Common.Domain.Localization;
using FourPrep.Modules.Catalogue.Domain.Costs;
using FourPrep.Modules.Catalogue.Application.Universities.ReplaceUniversityCosts;

public class ReplaceUniversityCostsCommandValidatorTests
{
    private static readonly ReplaceUniversityCostsCommandValidator Validator = new(new FakeTimeZoneContext());

    [Fact]
    public void Validate_WhenEachYearHasOneCost_Passes()
    {
        var command = Command(Cost(), Cost() with { AcademicYear = 2025, LivingMinor = null });

        Assert.True(Validator.Validate(command).IsValid);
    }

    [Fact]
    public void Validate_WhenTheListIsEmpty_Passes()
    {
        Assert.True(Validator.Validate(Command()).IsValid);
    }

    [Fact]
    public void Validate_WhenAnAcademicYearRepeats_FailsWithDuplicateAcademicYear()
    {
        AssertSingleError(Command(Cost(), Cost()), CostErrors.DuplicateAcademicYear.Code, "Costs");
    }

    [Theory]
    [InlineData(1999)]
    [InlineData(2101)]
    public void Validate_WhenTheAcademicYearIsImplausible_FailsWithInvalidAcademicYear(int academicYear)
    {
        AssertSingleError(
            Command(Cost() with { AcademicYear = academicYear }),
            CostErrors.InvalidAcademicYear.Code,
            "Costs[0].AcademicYear");
    }

    [Fact]
    public void Validate_WhenTheTuitionIsNegative_FailsWithInvalidAmount()
    {
        AssertSingleError(Command(Cost() with { TuitionMinor = -1 }), CostErrors.InvalidAmount.Code, "Costs[0].TuitionMinor");
    }

    [Fact]
    public void Validate_WhenTheLivingCostIsNegative_FailsWithInvalidAmount()
    {
        AssertSingleError(Command(Cost() with { LivingMinor = -1 }), CostErrors.InvalidAmount.Code, "Costs[0].LivingMinor");
    }

    [Theory]
    [InlineData("US$")]
    [InlineData("XYZ")]
    public void Validate_WhenTheCurrencyIsNotAnIsoCode_FailsWithInvalidCurrency(string currency)
    {
        AssertSingleError(
            Command(Cost() with { Currency = currency }),
            LocalizationErrors.InvalidCurrency.Code,
            "Costs[0].Currency");
    }

    private static CostInput Cost() =>
        new(2026, 5_773_000, 2_000_000, "usd", "https://sfs.mit.edu/cost", new DateOnly(2026, 9, 1));

    private static ReplaceUniversityCostsCommand Command(params CostInput[] costs) => new(Guid.NewGuid(), costs);

    private static void AssertSingleError(ReplaceUniversityCostsCommand command, string errorCode, string propertyName)
    {
        var error = Assert.Single(Validator.Validate(command).Errors);

        Assert.Equal(errorCode, error.ErrorCode);
        Assert.Equal(propertyName, error.PropertyName);
    }
}
