namespace FourPrep.Modules.Catalogue.UnitTests.Universities;

using FourPrep.Common.Domain.Enums;
using FourPrep.Modules.Catalogue.Application.Universities.GetUniversities;

public class UniversityFilterTests
{
    [Fact]
    public void BudgetCurrency_IsTheNormalizedCurrencyWhenABudgetIsGiven()
    {
        Assert.Equal("EUR", new UniversityFilter(Budget: 1_500_000, Currency: " eur ").BudgetCurrency);
    }

    [Theory]
    [InlineData(null, "EUR")]
    [InlineData(1_500_000L, null)]
    [InlineData(1_500_000L, "EURO")]
    public void BudgetCurrency_IsEmptyWithoutABudgetAndAValidCurrency(long? budget, string? currency)
    {
        Assert.Null(new UniversityFilter(Budget: budget, Currency: currency).BudgetCurrency);
    }

    [Fact]
    public void MajorPattern_EscapesLikeWildcards()
    {
        Assert.Equal(@"%data\_science%", new UniversityFilter(Major: " data_science ").MajorPattern);
    }

    [Fact]
    public void HasProgramConditions_IsTrueWhenAProgramFilterIsGiven()
    {
        Assert.False(UniversityFilter.None.HasProgramConditions);
        Assert.True(new UniversityFilter(DegreeLevel: DegreeLevel.Bachelor).HasProgramConditions);
        Assert.True(new UniversityFilter(Sat: 1400).HasProgramConditions);
    }
}
