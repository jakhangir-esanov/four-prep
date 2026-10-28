namespace FourPrep.Modules.Catalogue.UnitTests.Programs;

using FourPrep.Common.Domain.Enums;
using FourPrep.Modules.Catalogue.Domain.Programs;
using FourPrep.Modules.Catalogue.Application.Programs.UpdateProgram;

public class ProgramDetailsValidatorTests
{
    private static readonly UpdateProgramCommandValidator Validator = new(new FakeTimeZoneContext());

    [Fact]
    public void Validate_WhenTheProgramIsComplete_Passes()
    {
        Assert.True(Validator.Validate(Command()).IsValid);
    }

    [Theory]
    [InlineData("eng")]
    [InlineData("e1")]
    [InlineData("")]
    public void Validate_WhenTheLanguageIsNotAnIsoCode_FailsOnTheLanguage(string language)
    {
        var error = Assert.Single(Validator.Validate(Command() with { Details = ProgramCommandHandlerTests.Details() with { Language = language } }).Errors);

        Assert.Equal("Details.Language", error.PropertyName);
    }

    [Fact]
    public void Validate_WhenTheLanguageHasThreeLetters_FailsWithInvalidLanguage()
    {
        var error = Assert.Single(Validator.Validate(Command() with { Details = ProgramCommandHandlerTests.Details() with { Language = "deu" } }).Errors);

        Assert.Equal(ProgramErrors.InvalidLanguage.Code, error.ErrorCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(121)]
    public void Validate_WhenTheDurationIsImplausible_FailsWithInvalidDuration(int months)
    {
        var error = Assert.Single(Validator.Validate(Command() with { Details = ProgramCommandHandlerTests.Details() with { DurationMonths = months } }).Errors);

        Assert.Equal(ProgramErrors.InvalidDuration.Code, error.ErrorCode);
        Assert.Equal("Details.DurationMonths", error.PropertyName);
    }

    [Fact]
    public void Validate_WhenTheDegreeLevelIsUnknown_Fails()
    {
        var error = Assert.Single(Validator.Validate(Command() with { Details = ProgramCommandHandlerTests.Details() with { DegreeLevel = (DegreeLevel)0 } }).Errors);

        Assert.Equal("Details.DegreeLevel", error.PropertyName);
    }

    private static UpdateProgramCommand Command() => new(Guid.NewGuid(), ProgramCommandHandlerTests.Details());
}
