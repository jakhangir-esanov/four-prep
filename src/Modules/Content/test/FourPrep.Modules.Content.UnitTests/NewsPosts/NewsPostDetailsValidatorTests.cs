namespace FourPrep.Modules.Content.UnitTests.NewsPosts;

using FourPrep.Common.Domain.Localization;
using FourPrep.Modules.Content.Domain.NewsPosts;
using FourPrep.Modules.Content.Application.NewsPosts;
using FourPrep.Modules.Content.Application.NewsPosts.CreateNewsPost;
using FourPrep.Modules.Content.Application.NewsPosts.UpdateNewsPost;
using FourPrep.Modules.Content.Application.NewsPosts.SetNewsPostCover;

public class NewsPostDetailsValidatorTests
{
    private static readonly CreateNewsPostCommandValidator Validator = new();

    [Fact]
    public void Validate_WhenThePostIsComplete_Passes()
    {
        Assert.True(Validator.Validate(new CreateNewsPostCommand(NewsPostSamples.Details())).IsValid);
    }

    [Fact]
    public void Validate_WhenThePostIsOnlyInUzbek_Passes()
    {
        var details = NewsPostSamples.Details() with
        {
            Title = new Dictionary<string, string> { ["uz"] = "Yangi guruh" },
            Body = new Dictionary<string, string> { ["uz"] = "Noyabrda ochiladi." },
            Tag = null
        };

        Assert.True(Validator.Validate(new CreateNewsPostCommand(details)).IsValid);
    }

    [Theory]
    [InlineData("IELTS-results")]
    [InlineData("ielts results")]
    [InlineData("ielts--results")]
    [InlineData("-ielts")]
    public void Validate_WhenTheSlugIsNotAnAddressName_FailsWithInvalidSlug(string slug)
    {
        AssertSingleError(NewsPostSamples.Details() with { Slug = slug }, NewsPostErrors.InvalidSlug.Code, "Details.Slug");
    }

    [Fact]
    public void Validate_WhenTheSlugIsEmpty_Fails()
    {
        var errors = Validator.Validate(new CreateNewsPostCommand(NewsPostSamples.Details() with { Slug = " " })).Errors;

        Assert.Contains(errors, error => error.PropertyName == "Details.Slug");
    }

    [Fact]
    public void Validate_WhenTheTitleIsMissing_FailsWithTitleRequired()
    {
        var details = NewsPostSamples.Details() with
        {
            Title = new Dictionary<string, string> { ["en"] = " " },
            Body = null
        };

        AssertSingleError(details, NewsPostErrors.TitleRequired.Code, "Details.Title");
    }

    [Fact]
    public void Validate_WhenATextUsesAnUnsupportedLanguage_FailsWithUnsupportedLocale()
    {
        var details = NewsPostSamples.Details() with
        {
            Title = new Dictionary<string, string> { ["de"] = "Ergebnisse" },
            Body = new Dictionary<string, string> { ["de"] = "Text" }
        };

        var errors = Validator.Validate(new CreateNewsPostCommand(details)).Errors;

        Assert.Equal(["Details.Body", "Details.Title"], errors.Select(error => error.PropertyName).Order());
        Assert.All(errors, error => Assert.Equal(LocalizationErrors.UnsupportedLocale.Code, error.ErrorCode));
    }

    [Fact]
    public void Validate_WhenTheTitleIsTooLong_Fails()
    {
        var details = NewsPostSamples.Details() with
        {
            Title = new Dictionary<string, string> { ["en"] = new string('a', 201) },
            Body = new Dictionary<string, string> { ["en"] = "Text" }
        };

        var errors = Validator.Validate(new CreateNewsPostCommand(details)).Errors;

        Assert.Contains(errors, error => error.PropertyName == "Details.Title");
    }

    [Fact]
    public void Validate_WhenALanguageHasATitleButNoText_FailsWithTextLanguagesMismatch()
    {
        var details = NewsPostSamples.Details() with
        {
            Body = new Dictionary<string, string> { ["en"] = "Only English text." }
        };

        AssertSingleError(details, NewsPostErrors.TextLanguagesMismatch.Code, "Details.Body");
    }

    [Fact]
    public void Validate_WhenALanguageHasATextButNoTitle_FailsWithTextLanguagesMismatch()
    {
        var details = NewsPostSamples.Details() with
        {
            Title = new Dictionary<string, string> { ["en"] = "IELTS results" }
        };

        AssertSingleError(details, NewsPostErrors.TextLanguagesMismatch.Code, "Details.Body");
    }

    [Theory]
    [InlineData("Admissions")]
    [InlineData("admissions news")]
    [InlineData("qabul_2026")]
    public void Validate_WhenTheTagIsNotAKey_FailsWithInvalidTag(string tag)
    {
        AssertSingleError(NewsPostSamples.Details() with { Tag = tag }, NewsPostErrors.InvalidTag.Code, "Details.Tag");
    }

    [Fact]
    public void Validate_WhenUpdatingWithoutAnId_Fails()
    {
        var errors = new UpdateNewsPostCommandValidator()
            .Validate(new UpdateNewsPostCommand(Guid.Empty, NewsPostSamples.Details()))
            .Errors;

        Assert.Contains(errors, error => error.PropertyName == nameof(UpdateNewsPostCommand.Id));
    }

    [Fact]
    public void Validate_WhenTheCoverFileIsMissing_Fails()
    {
        var errors = new SetNewsPostCoverCommandValidator()
            .Validate(new SetNewsPostCoverCommand(Guid.NewGuid(), Guid.Empty))
            .Errors;

        Assert.Contains(errors, error => error.PropertyName == nameof(SetNewsPostCoverCommand.FileId));
    }

    private static void AssertSingleError(NewsPostDetails details, string code, string propertyName)
    {
        var error = Assert.Single(Validator.Validate(new CreateNewsPostCommand(details)).Errors);

        Assert.Equal(code, error.ErrorCode);
        Assert.Equal(propertyName, error.PropertyName);
    }
}
