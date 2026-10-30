namespace FourPrep.Modules.Content.UnitTests.NewsPosts;

using FourPrep.Modules.Content.Application.NewsPosts.GetNewsPosts;

public class NewsPostResponseTests
{
    [Fact]
    public void ForPublic_HidesTheCoverFileAndKeepsItsUrl()
    {
        var response = new NewsPostResponse
        {
            Slug = "ielts-results-2026",
            CoverFileId = Guid.NewGuid(),
            CoverUrl = "https://cdn.test/public/cover"
        };

        var forPublic = response.ForPublic();

        Assert.Null(forPublic.CoverFileId);
        Assert.Equal("https://cdn.test/public/cover", forPublic.CoverUrl);
        Assert.Equal("ielts-results-2026", forPublic.Slug);
    }

    [Fact]
    public void NewsPostFilter_NormalizesTheTagAndTheLanguage()
    {
        var filter = new NewsPostFilter(Tag: " Admissions ", Locale: " UZ ", Search: "  ");

        Assert.Equal("admissions", filter.NormalizedTag);
        Assert.Equal("uz", filter.NormalizedLocale);
        Assert.Null(filter.SearchPattern);
    }
}
