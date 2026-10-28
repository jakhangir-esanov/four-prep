namespace FourPrep.Modules.Auth.UnitTests.Authentication;

using FourPrep.Modules.Auth.Domain.Users;
using FourPrep.Common.Domain.Localization;
using FourPrep.Common.Domain.Authorization;
using FourPrep.Modules.Auth.Application.Authentication.Register;

public class RegisterCommandValidatorTests
{
    private static readonly RegisterCommandValidator Validator = new();

    [Theory]
    [InlineData("jahongir")]
    [InlineData("Jahongir_90")]
    [InlineData("ali.valiyev")]
    [InlineData("ali-valiyev")]
    [InlineData("ali@example.com")]
    public void Validate_WhenTheUsernameUsesAllowedCharacters_Passes(string username)
    {
        Assert.True(Validator.Validate(Command(username)).IsValid);
    }

    [Theory]
    [InlineData("محمد")]
    [InlineData("Жахонгир")]
    [InlineData("ali valiyev")]
    [InlineData("ali#1")]
    public void Validate_WhenTheUsernameHasCharactersKeycloakRejects_FailsWithInvalidUsername(string username)
    {
        var result = Validator.Validate(Command(username));

        var error = Assert.Single(result.Errors);
        Assert.Equal(nameof(RegisterCommand.Username), error.PropertyName);
        Assert.Equal(UserErrors.InvalidUsername.Code, error.ErrorCode);
    }

    [Fact]
    public void Validate_WhenTheUsernameIsEmpty_Fails()
    {
        Assert.False(Validator.Validate(Command("")).IsValid);
    }

    [Theory]
    [InlineData("US", "America/New_York", "en", "+14155550100")]
    [InlineData("gb", "Europe/London", "ru", "+44 20 7946 0958")]
    [InlineData("KZ", null, null, null)]
    [InlineData("UZ", "Asia/Tashkent", "uz", "+998 (90) 123-45-67")]
    public void Validate_WhenTheInternationalProfileIsValid_Passes(
        string country, string? timeZone, string? locale, string? phoneNumber)
    {
        var command = Command("student") with
        {
            Country = country,
            TimeZone = timeZone,
            Locale = locale,
            PhoneNumber = phoneNumber
        };

        Assert.True(Validator.Validate(command).IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("XX")]
    [InlineData("USA")]
    public void Validate_WhenTheCountryIsMissingOrUnknown_Fails(string country)
    {
        var result = Validator.Validate(Command("student") with { Country = country });

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(RegisterCommand.Country));
    }

    [Theory]
    [InlineData("Pacific Standard Time")]
    [InlineData("Mars/Olympus_Mons")]
    [InlineData("GMT+5")]
    public void Validate_WhenTheTimeZoneIsNotAnIanaName_FailsWithInvalidTimeZone(string timeZone)
    {
        var error = Assert.Single(Validator.Validate(Command("student") with { TimeZone = timeZone }).Errors);

        Assert.Equal(LocalizationErrors.InvalidTimeZone.Code, error.ErrorCode);
    }

    [Fact]
    public void Validate_WhenTheLocaleIsNotSupported_FailsWithUnsupportedLocale()
    {
        var error = Assert.Single(Validator.Validate(Command("student") with { Locale = "de" }).Errors);

        Assert.Equal(LocalizationErrors.UnsupportedLocale.Code, error.ErrorCode);
    }

    [Theory]
    [InlineData("901234567")]
    [InlineData("+0123456789")]
    [InlineData("+1234567890123456")]
    [InlineData("call me")]
    public void Validate_WhenThePhoneNumberIsNotInternational_FailsWithInvalidPhoneNumber(string phoneNumber)
    {
        var error = Assert.Single(Validator.Validate(Command("student") with { PhoneNumber = phoneNumber }).Errors);

        Assert.Equal(LocalizationErrors.InvalidPhoneNumber.Code, error.ErrorCode);
    }

    [Fact]
    public void Validate_WhenThePasswordIsTheUsername_FailsWithPasswordNotAllowed()
    {
        var error = Assert.Single(Validator.Validate(Command("ali12345") with { Password = "ALI12345" }).Errors);

        Assert.Equal(nameof(RegisterCommand.Password), error.PropertyName);
        Assert.Equal(UserErrors.PasswordNotAllowed.Code, error.ErrorCode);
    }

    private static RegisterCommand Command(string username) =>
        new(username, "user@example.com", "+967771234567", "Passw0rd!23", "YE", "Asia/Aden", "en", Roles.Student);
}
