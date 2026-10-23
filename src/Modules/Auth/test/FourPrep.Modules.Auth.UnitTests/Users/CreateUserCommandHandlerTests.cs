namespace FourPrep.Modules.Auth.UnitTests.Users;

using FourPrep.Modules.Auth.Domain.Users;
using FourPrep.Modules.Auth.Domain.IdentityProvider;
using FourPrep.Modules.Auth.Application.Users.CreateUser;

public class CreateUserCommandHandlerTests
{
    private static readonly CreateUserCommand Command =
        new("aziz.karimov", "aziz@example.com", "+998901234567", "Passw0rd!23", "mentor");

    [Fact]
    public async Task Handle_CreatesAnActiveAccountWithTheRole()
    {
        var identity = new FakeIdentityProviderClient();
        var handler = new CreateUserCommandHandler(identity, new FakeStudentService());

        var result = await handler.Handle(Command, default);

        Assert.True(result.IsSuccess);
        var user = identity.Users[result.Data];
        Assert.Equal("aziz.karimov", user.UserName);
        Assert.Equal("aziz@example.com", user.Email);
        Assert.Equal("+998901234567", user.PhoneNumber);
        Assert.True(user.IsActive);
        Assert.Equal((result.Data, "mentor"), Assert.Single(identity.AssignedRoles));
    }

    [Fact]
    public async Task Handle_ReportsATakenUsernameWithoutAssigningARole()
    {
        var identity = new FakeIdentityProviderClient { CreateFailure = UserErrors.UsernameTaken };
        var handler = new CreateUserCommandHandler(identity, new FakeStudentService());

        var result = await handler.Handle(Command, default);

        Assert.Equal(UserErrors.UsernameTaken, result.Error);
        Assert.Empty(identity.AssignedRoles);
    }

    [Fact]
    public async Task Handle_DeletesTheAccountWhenTheRoleIsRefused()
    {
        var identity = new FakeIdentityProviderClient { AssignRoleFailure = IdentityProviderErrors.RoleNotFound };
        var handler = new CreateUserCommandHandler(identity, new FakeStudentService());

        var result = await handler.Handle(Command, default);

        Assert.Equal(IdentityProviderErrors.RoleNotFound, result.Error);
        Assert.Empty(identity.Users);
    }
}
