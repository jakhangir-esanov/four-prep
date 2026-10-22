namespace FourPrep.Modules.Auth.UnitTests.Users;

using FourPrep.Modules.Auth.Domain.Users;
using FourPrep.Modules.Auth.Application.Users.UnblockUser;

public class UnblockUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_EnablesADisabledAccount()
    {
        var identity = new FakeIdentityProviderClient();
        var user = identity.Add("ali", isActive: false);

        var result = await new UnblockUserCommandHandler(identity).Handle(new UnblockUserCommand(user.Id), default);

        Assert.True(result.IsSuccess);
        Assert.True(identity.Users[user.Id].IsActive);
    }

    [Fact]
    public async Task Handle_RefusesAnAccountThatIsNotBlocked()
    {
        var identity = new FakeIdentityProviderClient();
        var user = identity.Add("ali", isActive: true);

        var result = await new UnblockUserCommandHandler(identity).Handle(new UnblockUserCommand(user.Id), default);

        Assert.Equal(UserErrors.NotBlocked, result.Error);
    }
}
