namespace FourPrep.Modules.Auth.UnitTests.Users;

using FourPrep.Modules.Auth.Domain.Users;
using FourPrep.Common.Domain.Authorization;
using FourPrep.Modules.Auth.Application.Users.CreateUser;

public class CreateUserCommandValidatorTests
{
    private static readonly CreateUserCommandValidator Validator = new();

    public static TheoryData<string> KnownRoles => [.. Roles.All];

    [Theory]
    [MemberData(nameof(KnownRoles))]
    public void Validate_WhenTheRoleIsKnown_Passes(string role)
    {
        Assert.True(Validator.Validate(Command(role)).IsValid);
    }

    [Theory]
    [InlineData("trainer")]
    [InlineData("admin")]
    [InlineData("Student")]
    public void Validate_WhenTheRoleIsUnknown_FailsWithInvalidRole(string role)
    {
        var result = Validator.Validate(Command(role));

        var error = Assert.Single(result.Errors);
        Assert.Equal(nameof(CreateUserCommand.Role), error.PropertyName);
        Assert.Equal(UserErrors.InvalidRole.Code, error.ErrorCode);
    }

    private static CreateUserCommand Command(string role) =>
        new("aziz.karimov", "aziz@example.com", null, "Passw0rd!23", role);
}
