namespace FourPrep.Modules.Auth.UnitTests.Users;

using FourPrep.Modules.Auth.Domain.Users;
using FourPrep.Modules.Auth.Application.Users.UpdateUserContact;

public class UpdateUserContactCommandHandlerTests
{
    [Fact]
    public async Task Handle_ChangesTheContactAndKeepsTheUsernameAndStatus()
    {
        var identity = new FakeIdentityProviderClient();
        var user = identity.Add("aziz", isActive: true);
        var handler = new UpdateUserContactCommandHandler(identity);

        var result = await handler.Handle(
            new UpdateUserContactCommand(user.Id, "new@example.com", "+998901234567"), default);

        var updated = identity.Users[user.Id];
        Assert.True(result.IsSuccess);
        Assert.Equal("aziz", updated.UserName);
        Assert.Equal("new@example.com", updated.Email);
        Assert.Equal("+998901234567", updated.PhoneNumber);
        Assert.True(updated.IsActive);
    }

    [Fact]
    public async Task Handle_DropsEmailVerificationWhenTheEmailChanges()
    {
        var identity = new FakeIdentityProviderClient();
        var user = identity.Add("aziz", isActive: true);
        var handler = new UpdateUserContactCommandHandler(identity);

        await handler.Handle(new UpdateUserContactCommand(user.Id, "new@example.com", null), default);

        Assert.False(identity.Users[user.Id].EmailVerified);
    }

    [Fact]
    public async Task Handle_KeepsEmailVerificationWhenOnlyThePhoneChanges()
    {
        var identity = new FakeIdentityProviderClient();
        var user = identity.Add("aziz", isActive: true);
        var handler = new UpdateUserContactCommandHandler(identity);

        await handler.Handle(new UpdateUserContactCommand(user.Id, "AZIZ@example.com", "+998901234567"), default);

        Assert.True(identity.Users[user.Id].EmailVerified);
    }

    [Fact]
    public async Task Handle_ReportsAnUnknownAccount()
    {
        var handler = new UpdateUserContactCommandHandler(new FakeIdentityProviderClient());

        var result = await handler.Handle(
            new UpdateUserContactCommand(Guid.NewGuid(), "new@example.com", null), default);

        Assert.Equal(UserErrors.NotFound, result.Error);
    }
}
