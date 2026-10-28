namespace FourPrep.Modules.Academy.UnitTests.Courses;

using FourPrep.Common.Domain.Localization;
using FourPrep.Modules.Academy.Domain.Courses;
using FourPrep.Modules.Academy.Application.Courses;
using FourPrep.Modules.Academy.Application.Courses.CreateCourse;
using FourPrep.Modules.Academy.Application.Courses.UpdateCourse;

public class CourseDetailsValidatorTests
{
    private static readonly CreateCourseCommandValidator Validator = new();

    [Fact]
    public void Validate_WhenTheCourseIsComplete_Passes()
    {
        Assert.True(Validator.Validate(new CreateCourseCommand(CourseCommandHandlerTests.Details())).IsValid);
    }

    [Fact]
    public void Validate_WhenOnlyTheEnglishNameIsGiven_Passes()
    {
        var details = new CourseDetails("sat", new Dictionary<string, string> { ["en"] = "SAT" }, null);

        Assert.True(Validator.Validate(new CreateCourseCommand(details)).IsValid);
    }

    [Theory]
    [InlineData("General-English")]
    [InlineData("general english")]
    [InlineData("general--english")]
    [InlineData("-sat")]
    public void Validate_WhenTheSlugIsNotAnAddressName_FailsWithInvalidSlug(string slug)
    {
        AssertSingleError(Details() with { Slug = slug }, CourseErrors.InvalidSlug.Code, "Details.Slug");
    }

    [Fact]
    public void Validate_WhenTheSlugIsEmpty_Fails()
    {
        var errors = Validator.Validate(new CreateCourseCommand(Details() with { Slug = "" })).Errors;

        Assert.Contains(errors, error => error.PropertyName == "Details.Slug");
    }

    [Fact]
    public void Validate_WhenTheNameHasNoEnglishVersion_FailsWithNameRequired()
    {
        var details = Details() with { Name = new Dictionary<string, string> { ["uz"] = "Umumiy ingliz tili" } };

        AssertSingleError(details, CourseErrors.NameRequired.Code, "Details.Name");
    }

    [Fact]
    public void Validate_WhenTheNameIsMissing_FailsWithNameRequired()
    {
        AssertSingleError(Details() with { Name = null }, CourseErrors.NameRequired.Code, "Details.Name");
    }

    [Fact]
    public void Validate_WhenTheDescriptionUsesAnUnsupportedLanguage_FailsWithUnsupportedLocale()
    {
        var details = Details() with { Description = new Dictionary<string, string> { ["en"] = "English", ["de"] = "Englisch" } };

        AssertSingleError(details, LocalizationErrors.UnsupportedLocale.Code, "Details.Description");
    }

    [Fact]
    public void Validate_WhenTheDescriptionHasNoEnglishVersion_FailsWithMissingEnglishText()
    {
        var details = Details() with { Description = new Dictionary<string, string> { ["ru"] = "Английский" } };

        AssertSingleError(details, LocalizationErrors.MissingEnglishText.Code, "Details.Description");
    }

    [Fact]
    public void Validate_WhenTheUpdateHasNoCourseId_Fails()
    {
        var errors = new UpdateCourseCommandValidator().Validate(new UpdateCourseCommand(Guid.Empty, Details())).Errors;

        Assert.Equal("Id", Assert.Single(errors).PropertyName);
    }

    private static CourseDetails Details() => CourseCommandHandlerTests.Details();

    private static void AssertSingleError(CourseDetails details, string code, string propertyName)
    {
        var error = Assert.Single(Validator.Validate(new CreateCourseCommand(details)).Errors);

        Assert.Equal(code, error.ErrorCode);
        Assert.Equal(propertyName, error.PropertyName);
    }
}
