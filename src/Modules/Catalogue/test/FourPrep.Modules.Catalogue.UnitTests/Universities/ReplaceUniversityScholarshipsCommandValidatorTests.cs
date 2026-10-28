namespace FourPrep.Modules.Catalogue.UnitTests.Universities;

using FourPrep.Common.Domain.Enums;
using FourPrep.Common.Domain.Localization;
using FourPrep.Modules.Catalogue.Application.Universities.ReplaceUniversityScholarships;

public class ReplaceUniversityScholarshipsCommandValidatorTests
{
    private static readonly ReplaceUniversityScholarshipsCommandValidator Validator = new(new FakeTimeZoneContext());

    [Fact]
    public void Validate_WhenTheScholarshipIsComplete_Passes()
    {
        Assert.True(Validator.Validate(Command(Scholarship())).IsValid);
    }

    [Fact]
    public void Validate_WhenTheKindIsUnknown_Fails()
    {
        var error = Assert.Single(Validator.Validate(Command(Scholarship() with { Kind = (ScholarshipKind)99 })).Errors);

        Assert.Equal("Scholarships[0].Kind", error.PropertyName);
    }

    [Fact]
    public void Validate_WhenTheNameIsEmpty_Fails()
    {
        var error = Assert.Single(Validator.Validate(Command(Scholarship() with { Name = "" })).Errors);

        Assert.Equal("Scholarships[0].Name", error.PropertyName);
    }

    [Fact]
    public void Validate_WhenTheEligibilityHasNoEnglishVersion_FailsWithMissingEnglishText()
    {
        var command = Command(Scholarship() with
        {
            Eligibility = new Dictionary<string, string> { ["ru"] = "Для иностранных студентов" }
        });

        var error = Assert.Single(Validator.Validate(command).Errors);

        Assert.Equal(LocalizationErrors.MissingEnglishText.Code, error.ErrorCode);
        Assert.Equal("Scholarships[0].Eligibility", error.PropertyName);
    }

    [Fact]
    public void Validate_WhenTheSourceIsMissing_FailsOnTheSource()
    {
        var error = Assert.Single(Validator.Validate(Command(Scholarship() with { SourceUrl = "" })).Errors);

        Assert.Equal("Scholarships[0].SourceUrl", error.PropertyName);
    }

    private static ScholarshipInput Scholarship() =>
        new(
            "MIT Scholarship",
            ScholarshipKind.NeedBased,
            new Dictionary<string, string> { ["en"] = "Up to full tuition" },
            new Dictionary<string, string> { ["en"] = "Families earning under USD 200,000", ["uz"] = "Yillik daromadi 200 000 dollardan kam oilalar" },
            "https://sfs.mit.edu/scholarships",
            new DateOnly(2026, 9, 1));

    private static ReplaceUniversityScholarshipsCommand Command(params ScholarshipInput[] scholarships) =>
        new(Guid.NewGuid(), scholarships);
}
