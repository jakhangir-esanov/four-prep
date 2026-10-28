namespace FourPrep.Modules.Auth.UnitTests.Account;

using FourPrep.Modules.Auth.Domain.Users;
using FourPrep.Modules.Auth.Application.Account.ChangeMyPassword;

public class ChangeMyPasswordCommandValidatorTests
{
    private static readonly ChangeMyPasswordCommandValidator Validator = new();

    [Fact]
    public void Validate_WhenTheNewPasswordIsDifferentAndLongEnough_Passes()
    {
        Assert.True(Validator.Validate(new ChangeMyPasswordCommand("OldPassw0rd", "NewPassw0rd")).IsValid);
    }

    [Fact]
    public void Validate_WhenTheNewPasswordEqualsTheCurrentOne_FailsWithPasswordUnchanged()
    {
        var error = Assert.Single(Validator.Validate(new ChangeMyPasswordCommand("SamePassw0rd", "SamePassw0rd")).Errors);

        Assert.Equal(nameof(ChangeMyPasswordCommand.NewPassword), error.PropertyName);
        Assert.Equal(UserErrors.PasswordUnchanged.Code, error.ErrorCode);
    }

    [Fact]
    public void Validate_WhenTheNewPasswordIsTooShort_Fails()
    {
        var error = Assert.Single(Validator.Validate(new ChangeMyPasswordCommand("OldPassw0rd", "short")).Errors);

        Assert.Equal(nameof(ChangeMyPasswordCommand.NewPassword), error.PropertyName);
    }

    [Fact]
    public void Validate_WhenTheCurrentPasswordIsMissing_Fails()
    {
        var error = Assert.Single(Validator.Validate(new ChangeMyPasswordCommand("", "NewPassw0rd")).Errors);

        Assert.Equal(nameof(ChangeMyPasswordCommand.CurrentPassword), error.PropertyName);
    }
}
