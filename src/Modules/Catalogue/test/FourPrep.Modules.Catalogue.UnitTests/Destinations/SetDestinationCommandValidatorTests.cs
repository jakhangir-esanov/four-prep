namespace FourPrep.Modules.Catalogue.UnitTests.Destinations;

using FourPrep.Common.Domain.Localization;
using FourPrep.Modules.Catalogue.Application.Destinations.SetDestination;

public class SetDestinationCommandValidatorTests
{
    private static readonly SetDestinationCommandValidator Validator = new();

    [Fact]
    public void Validate_WhenTheCountryAndVisaAreGiven_Passes()
    {
        Assert.True(Validator.Validate(new SetDestinationCommand("US", "F-1")).IsValid);
    }

    [Theory]
    [InlineData("USA")]
    [InlineData("XX")]
    public void Validate_WhenTheCountryIsNotAnIsoCode_FailsWithInvalidCountry(string countryCode)
    {
        var error = Assert.Single(Validator.Validate(new SetDestinationCommand(countryCode, "F-1")).Errors);

        Assert.Equal(LocalizationErrors.InvalidCountry.Code, error.ErrorCode);
        Assert.Equal(nameof(SetDestinationCommand.CountryCode), error.PropertyName);
    }

    [Fact]
    public void Validate_WhenTheVisaTypeIsEmpty_Fails()
    {
        var error = Assert.Single(Validator.Validate(new SetDestinationCommand("US", " ")).Errors);

        Assert.Equal(nameof(SetDestinationCommand.VisaType), error.PropertyName);
    }
}
