namespace FourPrep.Modules.Academy.UnitTests.ClassGroups;

using FourPrep.Common.Domain.Localization;
using FourPrep.Modules.Academy.Domain.ClassGroups;
using FourPrep.Modules.Academy.Domain.ScheduleSlots;
using FourPrep.Modules.Academy.Application.ClassGroups;
using FourPrep.Modules.Academy.Application.ClassGroups.CreateClassGroup;
using FourPrep.Modules.Academy.Application.ClassGroups.ChangeClassGroupStatus;

public class ClassGroupDetailsValidatorTests
{
    private static readonly CreateClassGroupCommandValidator Validator = new();

    [Fact]
    public void Validate_WhenTheOnlineGroupIsComplete_Passes()
    {
        Assert.True(Validate(ClassGroupDetailsSamples.Online()).IsValid);
    }

    [Fact]
    public void Validate_WhenTheOfficeGroupIsComplete_Passes()
    {
        Assert.True(Validate(ClassGroupDetailsSamples.Office()).IsValid);
    }

    [Theory]
    [InlineData("SAT 2026")]
    [InlineData("SAT--2026")]
    [InlineData("-SAT")]
    [InlineData("SAT_2026")]
    public void Validate_WhenTheCodeHasOtherCharacters_FailsWithInvalidCode(string code)
    {
        AssertSingleError(ClassGroupDetailsSamples.Online() with { Code = code }, ClassGroupErrors.InvalidCode.Code, "Details.Code");
    }

    [Theory]
    [InlineData(null, "London", "1 Example Street", "Details.CountryCode")]
    [InlineData("GB", null, "1 Example Street", "Details.City")]
    [InlineData("GB", "London", " ", "Details.Address")]
    public void Validate_WhenAnOfficeGroupHasNoFullLocation_FailsWithOfficeLocationRequired(
        string? countryCode,
        string? city,
        string? address,
        string propertyName)
    {
        var details = ClassGroupDetailsSamples.Office() with { CountryCode = countryCode, City = city, Address = address };

        AssertSingleError(details, ClassGroupErrors.OfficeLocationRequired.Code, propertyName);
    }

    [Fact]
    public void Validate_WhenTheCountryIsNotAnIsoCode_FailsWithInvalidCountry()
    {
        var details = ClassGroupDetailsSamples.Office() with { CountryCode = "England" };

        AssertSingleError(details, LocalizationErrors.InvalidCountry.Code, "Details.CountryCode");
    }

    [Fact]
    public void Validate_WhenTheTimeZoneIsNotAnIanaName_FailsWithInvalidTimeZone()
    {
        AssertSingleError(
            ClassGroupDetailsSamples.Online() with { TimeZone = "GMT+5" },
            LocalizationErrors.InvalidTimeZone.Code,
            "Details.TimeZone");
    }

    [Fact]
    public void Validate_WhenTheEntryLevelHasNoEnglishVersion_FailsWithMissingEnglishText()
    {
        var details = ClassGroupDetailsSamples.Online() with
        {
            EntryLevel = new Dictionary<string, string> { ["uz"] = "IELTS 5.0 yoki test" }
        };

        AssertSingleError(details, LocalizationErrors.MissingEnglishText.Code, "Details.EntryLevel");
    }

    [Theory]
    [InlineData(0, 10, "Details.Weeks")]
    [InlineData(105, 10, "Details.Weeks")]
    [InlineData(12, 0, "Details.Capacity")]
    [InlineData(12, 201, "Details.Capacity")]
    public void Validate_WhenTheLengthOrSizeIsOutOfRange_Fails(int weeks, int capacity, string propertyName)
    {
        var details = ClassGroupDetailsSamples.Online() with { Weeks = weeks, Capacity = capacity };

        Assert.Equal(propertyName, Assert.Single(Validate(details).Errors).PropertyName);
    }

    [Fact]
    public void Validate_WhenTheModeIsUnknown_Fails()
    {
        var details = ClassGroupDetailsSamples.Online() with { Mode = (ClassMode)9 };

        Assert.Equal("Details.Mode", Assert.Single(Validate(details).Errors).PropertyName);
    }

    [Fact]
    public void Validate_WhenTheScheduleIsEmpty_FailsWithScheduleRequired()
    {
        AssertSingleError(ClassGroupDetailsSamples.Online() with { Schedule = [] }, ClassGroupErrors.ScheduleRequired.Code, "Details.Schedule");
    }

    [Fact]
    public void Validate_WhenAScheduleItemIsMissing_Fails()
    {
        var details = ClassGroupDetailsSamples.Online() with
        {
            Schedule = [new(DayOfWeek.Monday, new TimeOnly(18, 0), 90), null!]
        };

        Assert.Equal("Details.Schedule[1]", Assert.Single(Validate(details).Errors).PropertyName);
    }

    [Fact]
    public void Validate_WhenTwoLessonsOverlap_FailsWithOverlapping()
    {
        var details = ClassGroupDetailsSamples.Online() with
        {
            Schedule =
            [
                new(DayOfWeek.Monday, new TimeOnly(18, 0), 90),
                new(DayOfWeek.Monday, new TimeOnly(19, 0), 60)
            ]
        };

        AssertSingleError(details, ScheduleSlotErrors.Overlapping.Code, "Details.Schedule");
    }

    [Fact]
    public void Validate_WhenLessonsFollowEachOther_Passes()
    {
        var details = ClassGroupDetailsSamples.Online() with
        {
            Schedule =
            [
                new(DayOfWeek.Monday, new TimeOnly(18, 0), 60),
                new(DayOfWeek.Monday, new TimeOnly(19, 0), 60)
            ]
        };

        Assert.True(Validate(details).IsValid);
    }

    [Theory]
    [InlineData(10, 0, 10)]
    [InlineData(10, 0, 241)]
    [InlineData(23, 30, 60)]
    public void Validate_WhenALessonIsTooShortTooLongOrEndsAfterMidnight_FailsWithInvalidDuration(
        int hour,
        int minute,
        int durationMinutes)
    {
        var details = ClassGroupDetailsSamples.Online() with
        {
            Schedule = [new(DayOfWeek.Friday, new TimeOnly(hour, minute), durationMinutes)]
        };

        AssertSingleError(details, ScheduleSlotErrors.InvalidDuration.Code, "Details.Schedule[0].DurationMinutes");
    }

    [Fact]
    public void Validate_WhenTheStatusIsUnknown_Fails()
    {
        var errors = new ChangeClassGroupStatusCommandValidator()
            .Validate(new ChangeClassGroupStatusCommand(Guid.NewGuid(), (ClassGroupStatus)9)).Errors;

        Assert.Equal(nameof(ChangeClassGroupStatusCommand.Status), Assert.Single(errors).PropertyName);
    }

    private static FluentValidation.Results.ValidationResult Validate(ClassGroupDetails details) =>
        Validator.Validate(new CreateClassGroupCommand(Guid.NewGuid(), details));

    private static void AssertSingleError(ClassGroupDetails details, string code, string propertyName)
    {
        var error = Assert.Single(Validate(details).Errors);

        Assert.Equal(code, error.ErrorCode);
        Assert.Equal(propertyName, error.PropertyName);
    }
}
