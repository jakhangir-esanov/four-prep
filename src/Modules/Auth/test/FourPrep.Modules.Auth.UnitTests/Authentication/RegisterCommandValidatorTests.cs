namespace FourPrep.Modules.Auth.UnitTests.Authentication;

using FourPrep.Modules.Auth.Domain.Users;
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

    private static RegisterCommand Command(string username) =>
        new(username, "user@example.com", "+967771234567", "Passw0rd!23");
}
