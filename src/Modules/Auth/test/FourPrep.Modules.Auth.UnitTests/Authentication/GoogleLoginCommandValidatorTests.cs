namespace FourPrep.Modules.Auth.UnitTests.Authentication;

using FourPrep.Modules.Auth.Domain.Users;
using FourPrep.Common.Domain.Localization;
using FourPrep.Common.Domain.Authorization;
using FourPrep.Modules.Auth.Application.Authentication.GoogleLogin;

public class GoogleLoginCommandValidatorTests
{
    private static readonly GoogleLoginCommandValidator Validator = new();

    [Fact]
    public void Validate_WhenOnlyTheTokenIsSent_Passes()
    {
        Assert.True(Validator.Validate(new GoogleLoginCommand("id-token", null, null, null, null)).IsValid);
    }

    [Theory]
    [InlineData(Roles.Student)]
    [InlineData(Roles.Parent)]
    public void Validate_WhenTheRoleIsOneASignUpMayChoose_Passes(string role)
    {
        Assert.True(Validator.Validate(new GoogleLoginCommand("id-token", role, "US", "Europe/London", "uz")).IsValid);
    }

    [Theory]
    [InlineData(Roles.Mentor)]
    [InlineData(Roles.SuperAdmin)]
    public void Validate_WhenTheRoleIsAStaffRole_FailsWithInvalidRole(string role)
    {
        var error = Assert.Single(Validator.Validate(new GoogleLoginCommand("id-token", role, "US", null, null)).Errors);

        Assert.Equal(UserErrors.InvalidRole.Code, error.ErrorCode);
    }

    [Fact]
    public void Validate_WhenTheCountryIsNotAnIsoCode_FailsWithInvalidCountry()
    {
        var error = Assert.Single(
            Validator.Validate(new GoogleLoginCommand("id-token", Roles.Student, "Uzbekistan", null, null)).Errors);

        Assert.Equal(LocalizationErrors.InvalidCountry.Code, error.ErrorCode);
    }

    [Fact]
    public void Validate_WhenTheTokenIsMissing_Fails()
    {
        Assert.False(Validator.Validate(new GoogleLoginCommand("", null, null, null, null)).IsValid);
    }
}
