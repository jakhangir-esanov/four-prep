namespace FourPrep.Modules.Auth.UnitTests.Users;

using FourPrep.Modules.Auth.Application.Users.DeleteUser;

public class DeleteUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_DeletesTheAccount()
    {
        var identity = new FakeIdentityProviderClient();
        var user = identity.Add("aziz", isActive: true);
        var handler = new DeleteUserCommandHandler(identity);

        var result = await handler.Handle(new DeleteUserCommand(user.Id), default);

        Assert.True(result.IsSuccess);
        Assert.Empty(identity.Users);
    }

    [Fact]
    public async Task Handle_TreatsAnAlreadyMissingAccountAsDeleted()
    {
        var handler = new DeleteUserCommandHandler(new FakeIdentityProviderClient());

        var result = await handler.Handle(new DeleteUserCommand(Guid.NewGuid()), default);

        Assert.True(result.IsSuccess);
    }
}
