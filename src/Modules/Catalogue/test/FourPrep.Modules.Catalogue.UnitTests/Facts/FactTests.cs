namespace FourPrep.Modules.Catalogue.UnitTests.Facts;

using FourPrep.Common.Domain.Localization;
using FourPrep.Modules.Catalogue.Application.Facts;

public class FactTests
{
    [Fact]
    public void CheckedBefore_IsTwelveMonthsBeforeToday()
    {
        Assert.Equal(new DateOnly(2025, 10, 23), StaleFacts.CheckedBefore(new FakeClock().UtcNow));
    }

    [Fact]
    public void Normalize_LowersLocalesTrimsTextAndDropsBlankEntries()
    {
        var text = new Dictionary<string, string> { [" EN "] = " Public university ", ["uz"] = "  ", ["Ru"] = "Университет" };

        var normalized = LocalizedTexts.Normalize(text);

        Assert.Equal(new Dictionary<string, string> { ["en"] = "Public university", ["ru"] = "Университет" }, normalized);
    }

    [Fact]
    public void Normalize_TurnsMissingTextIntoAnEmptyMap()
    {
        Assert.Empty(LocalizedTexts.Normalize(null));
    }
}
